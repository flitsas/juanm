using Gdc.Modules.Dgc.Domain.Common;

namespace Gdc.Modules.Dgc.Domain.Entities;

public sealed class Comparendo : TenantAuditableEntity
{
    public required string NumeroComparendo { get; set; }

    public required string Estado { get; set; }

    public string? InfractorNombre { get; set; }

    public string? Documento { get; set; }

    public string? Placa { get; set; }

    public string? InfraccionCodigo { get; set; }

    public DateOnly? FechaComparendo { get; set; }

    public DateOnly? FechaNotificacion { get; set; }

    public Guid? SecretariaId { get; set; }

    public decimal TotalValor { get; set; }

    public string? EstadoPago { get; set; }

    public string? DpReferencia { get; set; }

    public required string Fuente { get; set; }

    public bool PendienteContraventor { get; set; }

    public DateTimeOffset? UltimoIntentoAsociacion { get; set; }

    public Contraventor? Contraventor { get; set; }

    public ICollection<OcrItem> OcrItems { get; set; } = [];

    public ICollection<EmailLog> EmailLogs { get; set; } = [];
}
