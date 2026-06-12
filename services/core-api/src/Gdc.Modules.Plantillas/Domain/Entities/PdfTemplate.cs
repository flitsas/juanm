using Gdc.Modules.Plantillas.Domain.Common;

namespace Gdc.Modules.Plantillas.Domain.Entities;

public sealed class PdfTemplate : TenantAuditableEntity
{
    public required string Name { get; set; }

    public required string StorageKey { get; set; }

    public int Version { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }

    public ICollection<PdfTemplateField> Fields { get; set; } = [];

    public ICollection<DerechoPeticion> DerechosPeticion { get; set; } = [];
}
