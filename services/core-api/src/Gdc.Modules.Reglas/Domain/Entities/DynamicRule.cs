using Gdc.Modules.Reglas.Domain.Common;

namespace Gdc.Modules.Reglas.Domain.Entities;

public sealed class DynamicRule : TenantAuditableEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid PdfTemplateId { get; set; }

    public required string EmailSubject { get; set; }

    public required string EmailBodyHtml { get; set; }

    public Guid? SecretariatContactId { get; set; }

    public SecretariatContact? SecretariatContact { get; set; }

    public ICollection<RuleCondition> Conditions { get; set; } = [];

    public ICollection<RuleProcessingRecord> ProcessingRecords { get; set; } = [];
}
