namespace Gdc.Modules.Notif.Application.Abstractions;

public sealed record EmailLogWriteRequest(
    Guid TenantId,
    Guid ComparendoId,
    DateTimeOffset SentAt,
    string Origen,
    string Destino,
    string? Cc,
    string TipoAlerta,
    string EstadoEntrega,
    string? HtmlEvidencia);

public interface IEmailLogWriter
{
    Task<Guid> WriteAsync(EmailLogWriteRequest request, CancellationToken cancellationToken);
}
