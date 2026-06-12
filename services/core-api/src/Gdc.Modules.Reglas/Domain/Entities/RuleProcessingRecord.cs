using Gdc.Modules.Reglas.Domain.Common;

namespace Gdc.Modules.Reglas.Domain.Entities;

public sealed class RuleProcessingRecord : TenantAuditableEntity
{
    public Guid DynamicRuleId { get; set; }

    public DynamicRule? DynamicRule { get; set; }

    public Guid ComparendoId { get; set; }

    public Guid? RuleExecutionRunId { get; set; }

    public RuleExecutionRun? RuleExecutionRun { get; set; }

    public required string Status { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? PdfDocumentRef { get; set; }

    public string? EmailSendRef { get; set; }

    public string? ErrorMessage { get; set; }
}
