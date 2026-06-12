using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Execution;
using Gdc.Modules.Reglas.Application.Orchestration;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasOrchestrationJob(
    GdcDbContext db,
    IDgcComparendoReader comparendoReader,
    IReglasSecretariatResolver secretariatResolver,
    IGdcPdfTemplateRenderer pdfRenderer,
    IReglasEmailDispatcher emailDispatcher,
    IEmailLogWriter emailLogWriter,
    TimeProvider timeProvider,
    IOptions<ReglasExecutionOptions> options,
    ILogger<ReglasOrchestrationJob> logger)
{
    public async Task<ReglasProcessResponse> ProcessMatchesAsync(
        Guid tenantId,
        Guid? runId,
        CancellationToken cancellationToken)
    {
        var query = db.RuleProcessingRecords
            .Where(r => r.TenantId == tenantId
                        && r.Status == ReglasProcessingStatuses.Matched
                        && r.DeletedAt == null);

        if (runId.HasValue)
        {
            query = query.Where(r => r.RuleExecutionRunId == runId);
        }

        var batchSize = Math.Max(1, options.Value.BatchSize);
        var records = await query
            .OrderBy(r => r.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var succeeded = 0;
        var failed = 0;
        var runDeltas = new Dictionary<Guid, (int Succeeded, int Failed)>();

        foreach (var record in records)
        {
            var success = await ProcessRecordAsync(record, cancellationToken);
            if (success)
            {
                succeeded++;
            }
            else
            {
                failed++;
            }

            if (record.RuleExecutionRunId is Guid executionRunId)
            {
                if (!runDeltas.TryGetValue(executionRunId, out var delta))
                {
                    delta = (0, 0);
                }

                runDeltas[executionRunId] = success
                    ? (delta.Succeeded + 1, delta.Failed)
                    : (delta.Succeeded, delta.Failed + 1);
            }
        }

        await UpdateRunMetricsAsync(runDeltas, cancellationToken);

        logger.LogInformation(
            "REGLAS orchestration processed {Total} records ({Succeeded} success, {Failed} failed).",
            records.Count,
            succeeded,
            failed);

        return new ReglasProcessResponse(records.Count, succeeded, failed);
    }

    private async Task<bool> ProcessRecordAsync(RuleProcessingRecord record, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var rule = await db.DynamicRules
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == record.DynamicRuleId && r.TenantId == record.TenantId && r.DeletedAt == null,
                cancellationToken);

        if (rule is null)
        {
            await MarkFailedAsync(record, "Dynamic rule not found.", now, cancellationToken);
            return false;
        }

        var contact = await secretariatResolver.ResolveAsync(
            record.TenantId,
            record.ComparendoId,
            rule.SecretariatContactId,
            cancellationToken);

        if (contact is null || string.IsNullOrWhiteSpace(contact.ContactEmail))
        {
            await MarkFailedAsync(record, "Secretariat contact is not configured for this comparendo.", now, cancellationToken);
            return false;
        }

        var comparendos = await comparendoReader.ListByTenantAsync(record.TenantId, cancellationToken);
        var comparendo = comparendos.FirstOrDefault(c => c.Id == record.ComparendoId);
        if (comparendo is null)
        {
            await MarkFailedAsync(record, "Comparendo not found for processing.", now, cancellationToken);
            return false;
        }

        var tags = ReglasTagComposer.BuildComparendoTags(comparendo, rule.Name);

        PdfRenderResult pdf;
        try
        {
            pdf = await pdfRenderer.RenderAsync(rule.PdfTemplateId, tags, cancellationToken);
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(record, $"PDF render failed: {ex.Message}", now, cancellationToken);
            return false;
        }

        var subject = ReglasTagComposer.Merge(rule.EmailSubject, tags);
        var htmlBody = ReglasTagComposer.Merge(rule.EmailBodyHtml, tags);
        var pdfFileName = $"DP-{comparendo.NumeroComparendo}.pdf";

        var dispatch = await emailDispatcher.DispatchAsync(
            new ReglasEmailDispatchRequest(
                record.TenantId,
                record.ComparendoId,
                contact.ContactEmail,
                subject,
                htmlBody,
                pdf.Content,
                pdfFileName),
            cancellationToken);

        if (!dispatch.Success)
        {
            await MarkFailedAsync(record, dispatch.ErrorMessage ?? "Email dispatch failed.", now, cancellationToken);
            return false;
        }

        await emailLogWriter.WriteAsync(
            new EmailLogWriteRequest(
                record.TenantId,
                record.ComparendoId,
                now,
                "reglas@flit.dev",
                contact.ContactEmail,
                null,
                $"REGLAS:{rule.Name}",
                "Entregado",
                htmlBody),
            cancellationToken);

        record.Status = ReglasProcessingStatuses.Success;
        record.PdfDocumentRef = pdf.DocumentRef;
        record.EmailSendRef = dispatch.EmailSendRef;
        record.ProcessedAt = now;
        record.ErrorMessage = null;
        record.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task MarkFailedAsync(
        RuleProcessingRecord record,
        string error,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        record.Status = ReglasProcessingStatuses.Failed;
        record.ErrorMessage = error;
        record.ProcessedAt = now;
        record.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateRunMetricsAsync(
        Dictionary<Guid, (int Succeeded, int Failed)> runDeltas,
        CancellationToken cancellationToken)
    {
        if (runDeltas.Count == 0)
        {
            return;
        }

        var runIds = runDeltas.Keys.ToList();
        var runs = await db.RuleExecutionRuns
            .Where(r => runIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        foreach (var run in runs)
        {
            var (succeeded, failed) = runDeltas[run.Id];
            run.ProcessedCount += succeeded;
            run.FailedCount += failed;
            run.UpdatedAt = timeProvider.GetUtcNow();
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
