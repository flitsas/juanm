using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Reglas.Application.Execution;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasExecutionService(
    GdcDbContext db,
    DgcTenantContext tenantContext,
    ReglasExecutionJob executionJob,
    ReglasOrchestrationJob orchestrationJob)
{
    public async Task<ReglasRunListResponse> ListRunsAsync(CancellationToken cancellationToken)
    {
        var items = await db.RuleExecutionRuns
            .AsNoTracking()
            .Where(r => r.TenantId == tenantContext.TenantId && r.DeletedAt == null)
            .OrderByDescending(r => r.StartedAt)
            .Select(r => ToRunResponse(r))
            .ToListAsync(cancellationToken);

        return new ReglasRunListResponse(items);
    }

    public async Task<ReglasRunResponse?> GetRunAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await db.RuleExecutionRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == id && r.TenantId == tenantContext.TenantId && r.DeletedAt == null,
                cancellationToken);

        return run is null ? null : ToRunResponse(run);
    }

    public async Task<TriggerReglasRunResponse> TriggerManualRunAsync(CancellationToken cancellationToken)
    {
        var (runId, matchedCount) = await executionJob.RunForTenantAsync(
            tenantContext.TenantId,
            ReglasTriggerTypes.Manual,
            tenantContext.UserId,
            cancellationToken);

        return new TriggerReglasRunResponse(runId, matchedCount);
    }

    public Task<ReglasProcessResponse> ProcessRunAsync(Guid runId, CancellationToken cancellationToken) =>
        orchestrationJob.ProcessMatchesAsync(tenantContext.TenantId, runId, cancellationToken);

    public Task<ReglasProcessResponse> ProcessPendingMatchesAsync(CancellationToken cancellationToken) =>
        orchestrationJob.ProcessMatchesAsync(tenantContext.TenantId, runId: null, cancellationToken);

    public async Task<ReglasMatchListResponse> ListMatchesAsync(
        Guid? runId,
        Guid? ruleId,
        CancellationToken cancellationToken)
    {
        var query = db.RuleProcessingRecords
            .AsNoTracking()
            .Where(r => r.TenantId == tenantContext.TenantId && r.DeletedAt == null);

        if (runId.HasValue)
        {
            query = query.Where(r => r.RuleExecutionRunId == runId);
        }

        if (ruleId.HasValue)
        {
            query = query.Where(r => r.DynamicRuleId == ruleId);
        }

        var records = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
        {
            return new ReglasMatchListResponse([]);
        }

        var ruleIds = records.Select(r => r.DynamicRuleId).Distinct().ToList();
        var ruleNames = await db.DynamicRules
            .AsNoTracking()
            .Where(r => ruleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        var comparendoIds = records.Select(r => r.ComparendoId).Distinct().ToList();
        var comparendoNumbers = await db.Comparendos
            .AsNoTracking()
            .Where(c => comparendoIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NumeroComparendo, cancellationToken);

        var items = records
            .Select(r => new ReglasMatchResponse(
                r.Id,
                r.DynamicRuleId,
                ruleNames.GetValueOrDefault(r.DynamicRuleId, string.Empty),
                r.ComparendoId,
                comparendoNumbers.GetValueOrDefault(r.ComparendoId, string.Empty),
                r.RuleExecutionRunId,
                r.Status,
                r.ProcessedAt,
                r.CreatedAt))
            .ToList();

        return new ReglasMatchListResponse(items);
    }

    private static ReglasRunResponse ToRunResponse(RuleExecutionRun run) =>
        new(
            run.Id,
            run.StartedAt,
            run.FinishedAt,
            run.Status,
            run.TriggerType,
            run.EvaluatedCount,
            run.MatchedCount,
            run.ProcessedCount,
            run.FailedCount,
            run.ErrorMessage);
}
