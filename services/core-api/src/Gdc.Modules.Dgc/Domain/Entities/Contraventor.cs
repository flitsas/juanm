using Gdc.Modules.Dgc.Domain.Common;

namespace Gdc.Modules.Dgc.Domain.Entities;

public sealed class Contraventor : TenantAuditableEntity
{
    public Guid ComparendoId { get; set; }

    public Comparendo Comparendo { get; set; } = null!;

    public required string Nombre { get; set; }

    public required string Documento { get; set; }

    public string? Correo { get; set; }

    public bool AsociacionAutomatica { get; set; }
}
