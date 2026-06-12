using Gdc.Modules.Reglas.Domain.Common;

namespace Gdc.Modules.Reglas.Domain.Entities;

public sealed class RuleExecutionRun : TenantAuditableEntity
{
    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public required string Status { get; set; }

    public required string TriggerType { get; set; }

    public int EvaluatedCount { get; set; }

    public int MatchedCount { get; set; }

    public int ProcessedCount { get; set; }

    public int FailedCount { get; set; }

    public string? ErrorMessage { get; set; }

    public ICollection<RuleProcessingRecord> ProcessingRecords { get; set; } = [];
}
