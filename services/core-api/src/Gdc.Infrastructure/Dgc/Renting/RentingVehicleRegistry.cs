using Gdc.Modules.Dgc.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Gdc.Infrastructure.Dgc.Renting;

/// <summary>
/// Consulta contraventor en Renting por placa + fecha comparendo (paridad FLIT 1).
/// Regla RF06/RF07: si Renting trae nombre pero no customerId, se asocia igual; la cédula se completa manual.
/// </summary>
public sealed class RentingVehicleRegistry(
    RentingApiClient rentingApiClient,
    ILogger<RentingVehicleRegistry> logger) : IExternalVehicleRegistry
{
    public async Task<ContraventorLookupResult> LookupAsync(
        string placa,
        DateOnly fechaInfraccion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            return new ContraventorLookupResult(ContraventorLookupStatus.NotFound);
        }

        try
        {
            var contracts = await rentingApiClient.GetContractInfoByPlateAsync(
                placa,
                fechaInfraccion,
                cancellationToken);
            if (contracts is null or { Length: 0 })
            {
                return new ContraventorLookupResult(ContraventorLookupStatus.NotFound);
            }

            var first = contracts[0];
            var nombre = BuildFullName(first.Names, first.FirstSurname);
            if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(first.CustomerId))
            {
                return new ContraventorLookupResult(ContraventorLookupStatus.NotFound);
            }

            return new ContraventorLookupResult(
                ContraventorLookupStatus.Found,
                new ContraventorLookupData(
                    nombre,
                    first.CustomerId?.Trim() ?? string.Empty,
                    first.CustomerEmail?.Trim()));
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Renting API transport error for plate prefix {PlatePrefix}", MaskPlate(placa));
            return new ContraventorLookupResult(ContraventorLookupStatus.ProviderUnavailable);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Renting API timeout for plate prefix {PlatePrefix}", MaskPlate(placa));
            return new ContraventorLookupResult(ContraventorLookupStatus.ProviderUnavailable);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Renting API configuration error");
            return new ContraventorLookupResult(ContraventorLookupStatus.ProviderUnavailable);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected Renting API error for plate prefix {PlatePrefix}", MaskPlate(placa));
            return new ContraventorLookupResult(ContraventorLookupStatus.ProviderUnavailable);
        }
    }

    private static string BuildFullName(string? names, string? firstSurname)
    {
        var parts = new[] { names?.Trim(), firstSurname?.Trim() }
            .Where(static p => !string.IsNullOrWhiteSpace(p));
        return string.Join(' ', parts);
    }

    private static string MaskPlate(string plate)
    {
        var normalized = plate.Trim().ToUpperInvariant();
        return normalized.Length <= 3 ? "***" : $"{normalized[..3]}***";
    }
}
