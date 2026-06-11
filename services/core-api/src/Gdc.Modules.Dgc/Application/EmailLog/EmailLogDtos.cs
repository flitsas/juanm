namespace Gdc.Modules.Dgc.Application.EmailLog;

public sealed record EmailLogItemDto(
    Guid Id,
    DateTimeOffset SentAt,
    string Origen,
    string Destino,
    string? Cc,
    string TipoAlerta,
    string EstadoEntrega);

public sealed record EmailLogListResult(IReadOnlyList<EmailLogItemDto> Items);

public sealed record EmailEvidenceDto(Guid Id, string HtmlEvidencia);
