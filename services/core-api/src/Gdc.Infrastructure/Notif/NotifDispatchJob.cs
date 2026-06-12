using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Rules;
using Gdc.Modules.Notif.Application.Templates;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gdc.Infrastructure.Notif;

public sealed class NotifDispatchJob(
    GdcDbContext db,
    IDgcComparendoReader comparendoReader,
    IEmailSenderFactory senderFactory,
    IEmailCredentialEncryptor encryptor,
    IEmailLogWriter emailLogWriter,
    TimeProvider timeProvider,
    ILogger<NotifDispatchJob> logger)
{
    public async Task<int> RunCycleAsync(CancellationToken cancellationToken)
    {
        var tenantIds = await db.NotificationRules
            .AsNoTracking()
            .Where(r => r.IsActive && r.DeletedAt == null)
            .Select(r => r.TenantId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var enqueued = 0;
        foreach (var tenantId in tenantIds)
        {
            enqueued += await EnqueueEligibleAsync(tenantId, cancellationToken);
        }

        var dispatched = await DispatchPendingAsync(cancellationToken);
        logger.LogInformation("NOTIF dispatch cycle enqueued {Enqueued} and processed {Dispatched} items.", enqueued, dispatched);
        return enqueued + dispatched;
    }

    private async Task<int> EnqueueEligibleAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var dispatchEnabled = await db.EmailProviderConfigs
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive && p.DeletedAt == null)
            .Select(p => p.DispatchEnabled)
            .FirstOrDefaultAsync(cancellationToken);

        if (!dispatchEnabled)
        {
            return 0;
        }

        var rules = await db.NotificationRules
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.IsActive && r.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
        {
            return 0;
        }

        var comparendos = await comparendoReader.ListByTenantAsync(tenantId, cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var existingKeys = await db.EmailQueues
            .AsNoTracking()
            .Where(q => q.TenantId == tenantId && q.DeletedAt == null)
            .Select(q => new { q.NotificationRuleId, q.ComparendoId })
            .ToListAsync(cancellationToken);

        var existingSet = existingKeys
            .Select(k => $"{k.NotificationRuleId:N}:{k.ComparendoId:N}")
            .ToHashSet(StringComparer.Ordinal);

        var now = timeProvider.GetUtcNow();
        var created = 0;

        foreach (var rule in rules)
        {
            foreach (var comparendo in comparendos)
            {
                var key = $"{rule.Id:N}:{comparendo.Id:N}";
                if (existingSet.Contains(key) || !NotifRuleEvaluator.IsEligible(rule, comparendo, today))
                {
                    continue;
                }

                var queueItem = new EmailQueue
                {
                    Id = Guid.CreateVersion7(),
                    TenantId = tenantId,
                    NotificationRuleId = rule.Id,
                    EmailTemplateId = rule.EmailTemplateId,
                    ComparendoId = comparendo.Id,
                    Destino = comparendo.DestinoEmail!,
                    Status = QueueStatuses.Pending,
                    ScheduledAt = NotifRuleEvaluator.ResolveScheduledAt(rule, comparendo),
                    CreatedAt = now,
                };

                db.EmailQueues.Add(queueItem);
                existingSet.Add(key);
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return created;
    }

    private async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var pending = await db.EmailQueues
            .Include(q => q.NotificationRule)
            .Where(q => q.Status == QueueStatuses.Pending && q.ScheduledAt <= now && q.DeletedAt == null)
            .OrderBy(q => q.ScheduledAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return 0;
        }

        var processed = 0;
        foreach (var item in pending)
        {
            await DispatchItemAsync(item, cancellationToken);
            processed++;
        }

        return processed;
    }

    private async Task DispatchItemAsync(EmailQueue item, CancellationToken cancellationToken)
    {
        item.Status = QueueStatuses.Processing;
        item.UpdatedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);

        var provider = await db.EmailProviderConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == item.TenantId && p.IsActive && p.DeletedAt == null, cancellationToken);

        var template = await db.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == item.EmailTemplateId && t.TenantId == item.TenantId, cancellationToken);

        if (provider is null || template is null)
        {
            await MarkFailedAsync(item, "Missing provider or template configuration.", cancellationToken);
            return;
        }

        var comparendo = await comparendoReader.ListByTenantAsync(item.TenantId, cancellationToken);
        var snapshot = comparendo.FirstOrDefault(c => c.Id == item.ComparendoId);
        var variables = BuildVariables(snapshot, template.Name);
        var subject = TemplateHtmlComposer.MergeVariables(template.Subject, variables);
        var mergedBody = TemplateHtmlComposer.MergeVariables(template.HtmlBody, variables);
        var html = TemplateHtmlComposer.ComposeHtml(mergedBody, template.BannerUrl, template.FooterUrl);

        EmailSendResult sendResult;
        try
        {
            var credentialsJson = encryptor.Decrypt(provider.CredentialsEncrypted ?? string.Empty);
            var sender = senderFactory.Create(provider.ProviderType, credentialsJson);
            sendResult = await sender.SendAsync(
                new EmailMessage(provider.FromAddress, item.Destino, subject, html),
                cancellationToken);
        }
        catch (Exception ex)
        {
            sendResult = new EmailSendResult(false, null, ex.Message);
        }

        var sentAt = timeProvider.GetUtcNow();
        var deliveryStatus = sendResult.Success ? DeliveryStatuses.Delivered : DeliveryStatuses.Bounced;
        var dgcLogId = await emailLogWriter.WriteAsync(
            new EmailLogWriteRequest(
                item.TenantId,
                item.ComparendoId,
                sentAt,
                provider.FromAddress,
                item.Destino,
                null,
                template.Name,
                deliveryStatus,
                html),
            cancellationToken);

        var sendLog = new EmailSendLog
        {
            Id = Guid.CreateVersion7(),
            TenantId = item.TenantId,
            EmailQueueId = item.Id,
            ComparendoId = item.ComparendoId,
            Destino = item.Destino,
            Status = sendResult.Success ? QueueStatuses.Sent : QueueStatuses.Failed,
            SentAt = sentAt,
            ProviderMessageId = sendResult.ProviderMessageId,
            DgcEmailLogId = dgcLogId,
            CreatedAt = sentAt,
        };

        db.EmailSendLogs.Add(sendLog);
        item.Status = sendResult.Success ? QueueStatuses.Sent : QueueStatuses.Failed;
        item.ProcessedAt = sentAt;
        item.ErrorMessage = sendResult.ErrorMessage;
        item.UpdatedAt = sentAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkFailedAsync(EmailQueue item, string error, CancellationToken cancellationToken)
    {
        item.Status = QueueStatuses.Failed;
        item.ProcessedAt = timeProvider.GetUtcNow();
        item.ErrorMessage = error;
        item.UpdatedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, string> BuildVariables(DgcComparendoSnapshot? comparendo, string ruleName) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["infractor"] = comparendo?.InfractorNombre ?? "Contraventor",
            ["numero_comparendo"] = comparendo?.NumeroComparendo ?? string.Empty,
            ["placa"] = comparendo?.Placa ?? string.Empty,
            ["estado"] = comparendo?.Estado ?? string.Empty,
            ["tipo_alerta"] = ruleName,
        };
}
