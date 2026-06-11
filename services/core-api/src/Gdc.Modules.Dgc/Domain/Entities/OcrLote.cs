using Gdc.Modules.Dgc.Domain.Common;

namespace Gdc.Modules.Dgc.Domain.Entities;

public sealed class OcrLote : TenantAuditableEntity
{
    public required string Estado { get; set; }

    public ICollection<OcrItem> Items { get; set; } = [];
}
