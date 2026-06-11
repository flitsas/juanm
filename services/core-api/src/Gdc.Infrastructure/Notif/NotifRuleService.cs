using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Rules;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Notif;

public sealed class NotifRuleService(
    GdcDbContext db,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
{
    public async Task<RuleListResponse> ListAsync(CancellationToken cancellationToken)
    {
        var items = await db.NotificationRules
            .Where(r => r.TenantId == tenantContext.TenantId && r.DeletedAt == null)
            .OrderBy(r => r.Name)
            .Select(r => ToResponse(r))
            .ToListAsync(cancellationToken);

        return new RuleListResponse(items);
    }

    public async Task<RuleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var rule = await db.NotificationRules
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantContext.TenantId && r.DeletedAt == null, cancellationToken);

        return rule is null ? null : ToResponse(rule);
    }

    public async Task<RuleResponse> CreateAsync(CreateRuleRequest request, CancellationToken cancellationToken)
    {
        await ValidateRuleAsync(request.EmailTemplateId, request.TriggerType, request.TriggerDays,
            request.TriggerReference, request.TriggerEstado, request.IsActive, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var rule = new NotificationRule
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantContext.TenantId,
            EmailTemplateId = request.EmailTemplateId,
            Name = request.Name.Trim(),
            TriggerType = request.TriggerType.Trim(),
            TriggerDays = request.TriggerDays,
            TriggerReference = NormalizeOptional(request.TriggerReference),
            TriggerEstado = NormalizeOptional(request.TriggerEstado),
            IsActive = request.IsActive,
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.NotificationRules.Add(rule);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(rule);
    }

    public async Task<RuleResponse?> UpdateAsync(
        Guid id,
        UpdateRuleRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateRuleAsync(request.EmailTemplateId, request.TriggerType, request.TriggerDays,
            request.TriggerReference, request.TriggerEstado, request.IsActive, cancellationToken);

        var rule = await db.NotificationRules
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantContext.TenantId && r.DeletedAt == null, cancellationToken);

        if (rule is null)
        {
            return null;
        }

        rule.EmailTemplateId = request.EmailTemplateId;
        rule.Name = request.Name.Trim();
        rule.TriggerType = request.TriggerType.Trim();
        rule.TriggerDays = request.TriggerDays;
        rule.TriggerReference = NormalizeOptional(request.TriggerReference);
        rule.TriggerEstado = NormalizeOptional(request.TriggerEstado);
        rule.IsActive = request.IsActive;
        rule.UpdatedAt = timeProvider.GetUtcNow();
        rule.UpdatedBy = tenantContext.UserId;

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(rule);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var rule = await db.NotificationRules
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantContext.TenantId && r.DeletedAt == null, cancellationToken);

        if (rule is null)
        {
            return false;
        }

        rule.DeletedAt = timeProvider.GetUtcNow();
        rule.DeletedBy = tenantContext.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateRuleAsync(
        Guid emailTemplateId,
        string triggerType,
        int? triggerDays,
        string? triggerReference,
        string? triggerEstado,
        bool isActive,
        CancellationToken cancellationToken)
    {
        if (emailTemplateId == Guid.Empty)
        {
            throw new ArgumentException("EmailTemplateId is required.");
        }

        var templateExists = await db.EmailTemplates
            .AnyAsync(t => t.Id == emailTemplateId && t.TenantId == tenantContext.TenantId && t.DeletedAt == null, cancellationToken);

        if (!templateExists)
        {
            throw new ArgumentException("Associated email template was not found.");
        }

        if (isActive && emailTemplateId == Guid.Empty)
        {
            throw new ArgumentException("An active rule must reference a valid email template.");
        }

        if (!RuleTriggerTypes.Chronological.Equals(triggerType, StringComparison.OrdinalIgnoreCase)
            && !RuleTriggerTypes.State.Equals(triggerType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Unsupported trigger type: {triggerType}");
        }

        if (RuleTriggerTypes.Chronological.Equals(triggerType, StringComparison.OrdinalIgnoreCase))
        {
            if (triggerDays is null or < 0)
            {
                throw new ArgumentException("TriggerDays is required for chronological rules.");
            }

            var reference = NormalizeOptional(triggerReference) ?? TriggerReferences.FechaComparendo;
            if (!TriggerReferences.FechaComparendo.Equals(reference, StringComparison.OrdinalIgnoreCase)
                && !TriggerReferences.FechaNotificacion.Equals(reference, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Unsupported trigger reference: {reference}");
            }
        }

        if (RuleTriggerTypes.State.Equals(triggerType, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(triggerEstado))
        {
            throw new ArgumentException("TriggerEstado is required for state rules.");
        }
    }

    private static RuleResponse ToResponse(NotificationRule rule) =>
        new(
            rule.Id,
            rule.EmailTemplateId,
            rule.Name,
            rule.TriggerType,
            rule.TriggerDays,
            rule.TriggerReference,
            rule.TriggerEstado,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
