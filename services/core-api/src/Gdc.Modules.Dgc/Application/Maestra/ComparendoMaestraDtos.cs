namespace Gdc.Modules.Dgc.Application.Maestra;

public sealed record ComparendoMaestraItemDto(
    Guid Id,
    string Estado,
    string NumeroComparendo,
    string? Infractor,
    string? Documento,
    string? Placa,
    string? Infraccion,
    DateOnly? FechaComparendo,
    DateOnly? FechaNotificacion,
    int? DiasRestantes,
    string? Secretaria,
    decimal Total,
    string? Pago,
    string? Contraventor,
    string? ContraventorNombre,
    string? ContraventorDocumento,
    string? ContraventorCorreo,
    string? Dp,
    string Fuente);

public sealed record ComparendoMaestraListResult(
    IReadOnlyList<ComparendoMaestraItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record ComparendoMaestraQuery(
    string? Estado = null,
    string? Placa = null,
    string? Search = null,
    string? Secretaria = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    string SortBy = "fechaComparendo",
    string SortDir = "desc",
    int Page = 1,
    int PageSize = 20);
