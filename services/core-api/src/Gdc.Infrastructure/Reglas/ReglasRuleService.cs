using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Reglas.Application.Rules;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasRuleService(
    GdcDbContext db,
    DgcTenantContext tenantContext,
    ReglasAccessService access,
    TimeProvider timeProvider)
{
    public async Task<ReglasRuleListResponse> ListAsync(CancellationToken cancellationToken)
    {
        access.EnsureCanRead();

        var rules = await db.DynamicRules
            .Where(r => r.TenantId == tenantContext.TenantId && r.DeletedAt == null)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var ruleIds = rules.Select(r => r.Id).ToList();
        var conditions = await db.RuleConditions
            .Where(c => ruleIds.Contains(c.DynamicRuleId) && c.TenantId == tenantContext.TenantId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        var conditionsByRule = conditions.GroupBy(c => c.DynamicRuleId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<RuleCondition>)g.ToList());

        var items = rules
            .Select(rule => ToResponse(
                rule,
                conditionsByRule.GetValueOrDefault(rule.Id, [])))
            .ToList();

        return new ReglasRuleListResponse(items);
    }

    public async Task<ReglasRuleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        access.EnsureCanRead();

        var rule = await db.DynamicRules
            .FirstOrDefaultAsync(
                r => r.Id == id && r.TenantId == tenantContext.TenantId && r.DeletedAt == null,
                cancellationToken);

        if (rule is null)
        {
            return null;
        }

        var conditions = await LoadConditionsAsync(rule.Id, cancellationToken);
        return ToResponse(rule, conditions);
    }

    public async Task<ReglasRuleResponse> CreateAsync(
        CreateReglasRuleRequest request,
        CancellationToken cancellationToken)
    {
        access.EnsureCanManage();
        await ValidateReferencesAsync(request.SecretariatContactId, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var rule = new DynamicRule
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantContext.TenantId,
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            IsActive = request.IsActive,
            PdfTemplateId = request.PdfTemplateId,
            EmailSubject = request.EmailSubject.Trim(),
            EmailBodyHtml = request.EmailBodyHtml,
            SecretariatContactId = request.SecretariatContactId,
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.DynamicRules.Add(rule);

        var conditions = ReglasConditionTreeMapper.Flatten(
            rule.Id,
            tenantContext.TenantId,
            tenantContext.UserId,
            now,
            request.ConditionRoot);

        if (conditions.Count > 0)
        {
            db.RuleConditions.AddRange(conditions);
        }

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(rule, conditions);
    }

    public async Task<ReglasRuleResponse?> UpdateAsync(
        Guid id,
        UpdateReglasRuleRequest request,
        CancellationToken cancellationToken)
    {
        access.EnsureCanManage();
        await ValidateReferencesAsync(request.SecretariatContactId, cancellationToken);

        var rule = await db.DynamicRules
            .FirstOrDefaultAsync(
                r => r.Id == id && r.TenantId == tenantContext.TenantId && r.DeletedAt == null,
                cancellationToken);

        if (rule is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        rule.Name = request.Name.Trim();
        rule.Description = NormalizeOptional(request.Description);
        rule.IsActive = request.IsActive;
        rule.PdfTemplateId = request.PdfTemplateId;
        rule.EmailSubject = request.EmailSubject.Trim();
        rule.EmailBodyHtml = request.EmailBodyHtml;
        rule.SecretariatContactId = request.SecretariatContactId;
        rule.UpdatedAt = now;
        rule.UpdatedBy = tenantContext.UserId;

        var existingConditions = await db.RuleConditions
            .Where(c => c.DynamicRuleId == rule.Id && c.TenantId == tenantContext.TenantId)
            .ToListAsync(cancellationToken);
        db.RuleConditions.RemoveRange(existingConditions);

        var conditions = ReglasConditionTreeMapper.Flatten(
            rule.Id,
            tenantContext.TenantId,
            tenantContext.UserId,
            now,
            request.ConditionRoot);

        if (conditions.Count > 0)
        {
            db.RuleConditions.AddRange(conditions);
        }

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(rule, conditions);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        access.EnsureCanManage();

        var rule = await db.DynamicRules
            .FirstOrDefaultAsync(
                r => r.Id == id && r.TenantId == tenantContext.TenantId && r.DeletedAt == null,
                cancellationToken);

        if (rule is null)
        {
            return false;
        }

        var now = timeProvider.GetUtcNow();
        rule.DeletedAt = now;
        rule.DeletedBy = tenantContext.UserId;

        var conditions = await db.RuleConditions
            .Where(c => c.DynamicRuleId == rule.Id && c.TenantId == tenantContext.TenantId)
            .ToListAsync(cancellationToken);

        foreach (var condition in conditions)
        {
            condition.DeletedAt = now;
            condition.DeletedBy = tenantContext.UserId;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateReferencesAsync(Guid? secretariatContactId, CancellationToken cancellationToken)
    {
        if (secretariatContactId is null)
        {
            return;
        }

        var exists = await db.SecretariatContacts
            .AnyAsync(
                c => c.Id == secretariatContactId
                     && c.TenantId == tenantContext.TenantId
                     && c.DeletedAt == null,
                cancellationToken);

        if (!exists)
        {
            throw new ArgumentException("Associated secretariat contact was not found.");
        }
    }

    private async Task<IReadOnlyList<RuleCondition>> LoadConditionsAsync(
        Guid ruleId,
        CancellationToken cancellationToken) =>
        await db.RuleConditions
            .Where(c => c.DynamicRuleId == ruleId && c.TenantId == tenantContext.TenantId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

    private static ReglasRuleResponse ToResponse(
        DynamicRule rule,
        IReadOnlyList<RuleCondition> conditions) =>
        new(
            rule.Id,
            rule.Name,
            rule.Description,
            rule.IsActive,
            rule.PdfTemplateId,
            rule.EmailSubject,
            rule.EmailBodyHtml,
            rule.SecretariatContactId,
            ReglasConditionTreeMapper.BuildTree(conditions),
            rule.CreatedAt,
            rule.UpdatedAt);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
