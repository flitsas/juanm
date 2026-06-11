using Gdc.Modules.Dgc.Domain.Common;

namespace Gdc.Modules.Dgc.Domain.Entities;

public sealed class DescuentoMatriz : TenantAuditableEntity
{
    public Guid? SecretariaId { get; set; }

    public int DiasDescuento { get; set; }

    public bool IsActive { get; set; } = true;
}
