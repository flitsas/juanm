using Gdc.Modules.Plantillas.Domain.Common;

namespace Gdc.Modules.Plantillas.Domain.Entities;

public sealed class PdfTemplateField : TenantAuditableEntity
{
    public Guid PdfTemplateId { get; set; }

    public PdfTemplate PdfTemplate { get; set; } = null!;

    public required string AcroformName { get; set; }

    public required string FieldType { get; set; }

    public string? SystemVariable { get; set; }

    public string? ChoiceOptionsJson { get; set; }

    public int SortOrder { get; set; }
}
