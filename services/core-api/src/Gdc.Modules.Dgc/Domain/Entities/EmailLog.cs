using Gdc.Modules.Dgc.Domain.Common;

namespace Gdc.Modules.Dgc.Domain.Entities;

public sealed class EmailLog : TenantAuditableEntity
{
    public Guid ComparendoId { get; set; }

    public Comparendo Comparendo { get; set; } = null!;

    public DateTimeOffset SentAt { get; set; }

    public required string Origen { get; set; }

    public required string Destino { get; set; }

    public string? Cc { get; set; }

    public required string TipoAlerta { get; set; }

    public required string EstadoEntrega { get; set; }

    public string? HtmlEvidencia { get; set; }
}
