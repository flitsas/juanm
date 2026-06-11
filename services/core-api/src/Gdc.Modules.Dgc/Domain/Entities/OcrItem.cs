using Gdc.Modules.Dgc.Domain.Common;

namespace Gdc.Modules.Dgc.Domain.Entities;

public sealed class OcrItem : TenantAuditableEntity
{
    public Guid OcrLoteId { get; set; }

    public OcrLote OcrLote { get; set; } = null!;

    public Guid? ComparendoId { get; set; }

    public Comparendo? Comparendo { get; set; }

    public required string ArchivoUri { get; set; }

    public required string Estado { get; set; }

    public string? OcrPayloadJson { get; set; }
}
