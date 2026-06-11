using Gdc.Modules.Dgc.Application.Abstractions;

namespace Gdc.Infrastructure.Dgc;

public sealed class StubVehicleRegistry : IExternalVehicleRegistry
{
    public Task<ContraventorLookupResult> LookupAsync(
        string placa,
        DateOnly fechaInfraccion,
        CancellationToken cancellationToken) =>
        Task.FromResult(new ContraventorLookupResult(ContraventorLookupStatus.NotFound));
}
