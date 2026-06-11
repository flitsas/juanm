using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Dgc.Application.Contraventor;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Dgc;

public sealed class ContraventorManualService(GdcDbContext db, DgcTenantContext tenantContext)
{
    public async Task<(bool Success, ContraventorResponse? Response, string? ErrorCode)> UpsertAsync(
        Guid comparendoId,
        UpsertContraventorRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Documento))
        {
            return (false, null, "DGC_CONTRAVENTOR_INVALID");
        }

        var tenantId = tenantContext.TenantId;
        var comparendo = await db.Comparendos
            .Include(c => c.Contraventor)
            .FirstOrDefaultAsync(c => c.Id == comparendoId && c.TenantId == tenantId, cancellationToken);

        if (comparendo is null)
        {
            return (false, null, "DGC_COMPARENDO_NOT_FOUND");
        }

        var nombre = request.Nombre.Trim();
        var documento = request.Documento.Trim();
        var correo = string.IsNullOrWhiteSpace(request.Correo) ? null : request.Correo.Trim();

        Contraventor contraventor;

        if (comparendo.Contraventor is not null)
        {
            contraventor = comparendo.Contraventor;
            contraventor.Nombre = nombre;
            contraventor.Documento = documento;
            contraventor.Correo = correo;
            contraventor.AsociacionAutomatica = false;
            contraventor.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            contraventor = new Contraventor
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantId,
                ComparendoId = comparendo.Id,
                Nombre = nombre,
                Documento = documento,
                Correo = correo,
                AsociacionAutomatica = false,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            db.Contraventors.Add(contraventor);
        }

        comparendo.PendienteContraventor = false;
        comparendo.UltimoIntentoAsociacion = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return (true, new ContraventorResponse(
            contraventor.Id,
            comparendo.Id,
            contraventor.Nombre,
            contraventor.Documento,
            contraventor.Correo,
            contraventor.AsociacionAutomatica), null);
    }
}
