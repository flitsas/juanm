using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Dgc;

public sealed class DgcComparendoReader(GdcDbContext db) : IDgcComparendoReader
{
    public async Task<IReadOnlyList<DgcComparendoSnapshot>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await db.Comparendos
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.DeletedAt == null)
            .Select(c => new DgcComparendoSnapshot(
                c.Id,
                c.TenantId,
                c.NumeroComparendo,
                c.Estado,
                c.InfractorNombre,
                c.Placa,
                c.FechaComparendo,
                c.FechaNotificacion,
                c.Contraventor != null ? c.Contraventor.Correo : null))
            .ToListAsync(cancellationToken);
    }
}
