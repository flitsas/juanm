using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Dgc;

public sealed class ContraventorAssociationJob(
    GdcDbContext db,
    IExternalVehicleRegistry vehicleRegistry)
{
    public async Task<int> ProcessTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var comparendos = await db.Comparendos
            .Include(c => c.Contraventor)
            .Where(c =>
                c.TenantId == tenantId
                && c.Contraventor == null
                && c.Placa != null
                && c.FechaComparendo != null)
            .ToListAsync(cancellationToken);

        var processed = 0;

        foreach (var comparendo in comparendos)
        {
            var lookup = await vehicleRegistry.LookupAsync(
                comparendo.Placa!,
                comparendo.FechaComparendo!.Value,
                cancellationToken);

            comparendo.UltimoIntentoAsociacion = DateTimeOffset.UtcNow;

            if (lookup.Status == ContraventorLookupStatus.Found && lookup.Data is not null)
            {
                db.Contraventors.Add(new Contraventor
                {
                    Id = Guid.CreateVersion7(),
                    TenantId = tenantId,
                    ComparendoId = comparendo.Id,
                    Nombre = lookup.Data.Nombre,
                    Documento = lookup.Data.Documento,
                    Correo = lookup.Data.Correo,
                    AsociacionAutomatica = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                });

                comparendo.PendienteContraventor = false;
            }
            else if (lookup.Status == ContraventorLookupStatus.NotFound)
            {
                comparendo.PendienteContraventor = true;
            }
            // ProviderUnavailable: conservar estado y reintentar en el siguiente ciclo.

            processed++;
        }

        if (processed > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }
}
