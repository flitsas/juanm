using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Evaluation;
using Gdc.Modules.Reglas.Application.Execution;
using Gdc.Modules.Reglas.Application.Rules;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasExecutionJob(
    GdcDbContext db,
    IDgcComparendoReader comparendoReader,
    TimeProvider timeProvider,
    IOptions<ReglasExecutionOptions> options,
    ILogger<ReglasExecutionJob> logger)
{
    public async Task<int> RunCycleAsync(CancellationToken cancellationToken)
    {
        var tenantIds = await db.DynamicRules
            .AsNoTracking()
            .Where(r => r.IsActive && r.DeletedAt == null)
            .Select(r => r.TenantId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalMatches = 0;
        foreach (var tenantId in tenantIds)
        {
            var result = await RunForTenantAsync(tenantId, ReglasTriggerTypes.Scheduled, null, cancellationToken);
            totalMatches += result.MatchedCount;
        }

        logger.LogInformation("REGLAS evaluation cycle recorded {MatchedCount} new matches.", totalMatches);
        return totalMatches;
    }

    public async Task<(Guid RunId, int MatchedCount)> RunForTenantAsync(
        Guid tenantId,
        string triggerType,
        Guid? triggeredBy,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var run = new RuleExecutionRun
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            StartedAt = now,
            Status = ReglasRunStatuses.Running,
            TriggerType = triggerType,
            CreatedAt = now,
            CreatedBy = triggeredBy,
        };

        db.RuleExecutionRuns.Add(run);

        try
        {
            var rules = await db.DynamicRules
                .AsNoTracking()
                .Where(r => r.TenantId == tenantId && r.IsActive && r.DeletedAt == null)
                .ToListAsync(cancellationToken);

            if (rules.Count == 0)
            {
                return await CompleteRunAsync(run, evaluated: 0, matched: 0, cancellationToken);
            }

            var ruleIds = rules.Select(r => r.Id).ToList();
            var conditions = await db.RuleConditions
                .AsNoTracking()
                .Where(c => ruleIds.Contains(c.DynamicRuleId) && c.TenantId == tenantId)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(cancellationToken);

            var conditionsByRule = conditions
                .GroupBy(c => c.DynamicRuleId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<RuleCondition>)g.ToList());

            var comparendos = await comparendoReader.ListByTenantAsync(tenantId, cancellationToken);
            var batchSize = Math.Max(1, options.Value.BatchSize);
            var comparendoBatch = comparendos.Take(batchSize).ToList();

            var skipPairs = await db.RuleProcessingRecords
                .AsNoTracking()
                .Where(r => r.TenantId == tenantId
                            && (r.Status == ReglasProcessingStatuses.Success
                                || r.Status == ReglasProcessingStatuses.Matched))
                .Select(r => new { r.DynamicRuleId, r.ComparendoId })
                .ToListAsync(cancellationToken);

            var skipSet = skipPairs
                .Select(p => $"{p.DynamicRuleId:N}:{p.ComparendoId:N}")
                .ToHashSet(StringComparer.Ordinal);

            var evaluated = 0;
            var matched = 0;

            foreach (var rule in rules)
            {
                var tree = ReglasConditionTreeMapper.BuildTree(
                    conditionsByRule.GetValueOrDefault(rule.Id, []));

                if (tree is null)
                {
                    continue;
                }

                foreach (var comparendo in comparendoBatch)
                {
                    evaluated++;
                    var pairKey = $"{rule.Id:N}:{comparendo.Id:N}";
                    if (skipSet.Contains(pairKey))
                    {
                        continue;
                    }

                    if (!ReglasRuleEvaluator.Matches(tree, comparendo))
                    {
                        continue;
                    }

                    var record = new RuleProcessingRecord
                    {
                        Id = Guid.CreateVersion7(),
                        TenantId = tenantId,
                        DynamicRuleId = rule.Id,
                        ComparendoId = comparendo.Id,
                        RuleExecutionRunId = run.Id,
                        Status = ReglasProcessingStatuses.Matched,
                        CreatedAt = now,
                        CreatedBy = triggeredBy,
                    };

                    db.RuleProcessingRecords.Add(record);
                    skipSet.Add(pairKey);
                    matched++;
                }
            }

            return await CompleteRunAsync(run, evaluated, matched, cancellationToken);
        }
        catch (Exception ex)
        {
            run.Status = ReglasRunStatuses.Failed;
            run.FinishedAt = timeProvider.GetUtcNow();
            run.ErrorMessage = ex.Message;
            run.UpdatedAt = run.FinishedAt;
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task<(Guid RunId, int MatchedCount)> CompleteRunAsync(
        RuleExecutionRun run,
        int evaluated,
        int matched,
        CancellationToken cancellationToken)
    {
        run.EvaluatedCount = evaluated;
        run.MatchedCount = matched;
        run.ProcessedCount = 0;
        run.FailedCount = 0;
        run.Status = ReglasRunStatuses.Completed;
        run.FinishedAt = timeProvider.GetUtcNow();
        run.UpdatedAt = run.FinishedAt;
        await db.SaveChangesAsync(cancellationToken);
        return (run.Id, matched);
    }
}
