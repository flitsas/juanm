using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Plantillas.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Plantillas;

public sealed class ComparendoCompilationSource(GdcDbContext db) : IComparendoCompilationSource
{
    public async Task<ComparendoCompilationSnapshot?> GetByIdAsync(
        Guid comparendoId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var comparendo = await db.Comparendos
            .AsNoTracking()
            .Include(c => c.Contraventor)
            .FirstOrDefaultAsync(
                c => c.Id == comparendoId && c.TenantId == tenantId && c.DeletedAt == null,
                cancellationToken);

        if (comparendo is null)
        {
            return null;
        }

        var tenantName = await db.TenantCompanies
            .AsNoTracking()
            .Where(t => t.Id == tenantId && t.DeletedAt == null)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "Tenant";

        ContraventorCompilationSnapshot? contraventor = comparendo.Contraventor is null
            ? null
            : new ContraventorCompilationSnapshot(
                comparendo.Contraventor.Nombre,
                comparendo.Contraventor.Documento,
                comparendo.Contraventor.Correo);

        return new ComparendoCompilationSnapshot(
            comparendo.Id,
            comparendo.PendienteContraventor,
            comparendo.NumeroComparendo,
            comparendo.Estado,
            comparendo.InfractorNombre,
            comparendo.Documento,
            comparendo.Placa,
            comparendo.FechaComparendo,
            comparendo.FechaNotificacion,
            comparendo.TotalValor,
            contraventor,
            tenantName,
            comparendo.SecretariaId?.ToString());
    }
}
