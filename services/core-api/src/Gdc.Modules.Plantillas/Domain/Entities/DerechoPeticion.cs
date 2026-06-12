using Gdc.Modules.Plantillas.Domain.Common;

namespace Gdc.Modules.Plantillas.Domain.Entities;

public sealed class DerechoPeticion : TenantAuditableEntity
{
    public Guid ComparendoId { get; set; }

    public Guid PdfTemplateId { get; set; }

    public PdfTemplate PdfTemplate { get; set; } = null!;

    public int TemplateVersion { get; set; }

    public required string Estado { get; set; }

    public required string OutputStorageKey { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }
}
