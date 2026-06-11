namespace Gdc.Modules.Dgc.Application.Abstractions;

public enum ContraventorLookupStatus
{
    Found,
    NotFound,
    ProviderUnavailable,
}

public sealed record ContraventorLookupData(string Nombre, string Documento, string? Correo);

public sealed record ContraventorLookupResult(
    ContraventorLookupStatus Status,
    ContraventorLookupData? Data = null);

public interface IExternalVehicleRegistry
{
    Task<ContraventorLookupResult> LookupAsync(
        string placa,
        DateOnly fechaInfraccion,
        CancellationToken cancellationToken);
}
