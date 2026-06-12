using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Reglas.Application.Abstractions;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasSecretariatResolver(GdcDbContext db) : IReglasSecretariatResolver
{
    public async Task<SecretariatContact?> ResolveAsync(
        Guid tenantId,
        Guid comparendoId,
        Guid? ruleContactId,
        CancellationToken cancellationToken)
    {
        var secretariaId = await db.Comparendos
            .AsNoTracking()
            .Where(c => c.Id == comparendoId && c.TenantId == tenantId && c.DeletedAt == null)
            .Select(c => c.SecretariaId)
            .FirstOrDefaultAsync(cancellationToken);

        if (secretariaId.HasValue)
        {
            var secretariaCode = secretariaId.Value.ToString();
            var bySecretaria = await db.SecretariatContacts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.TenantId == tenantId
                         && c.SecretariatCode == secretariaCode
                         && c.IsActive
                         && c.DeletedAt == null,
                    cancellationToken);

            if (bySecretaria is not null)
            {
                return bySecretaria;
            }
        }

        if (!ruleContactId.HasValue)
        {
            return null;
        }

        return await db.SecretariatContacts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == ruleContactId
                     && c.TenantId == tenantId
                     && c.IsActive
                     && c.DeletedAt == null,
                cancellationToken);
    }
}
