using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Auth;

/// <summary>
/// Mantiene <c>core.tenants</c> alineado con <c>identity.tenants</c> (compañías NOTIF).
/// Auth.users referencia core.tenants; sin esta fila, invite devuelve 404 TenantNotFound.
/// </summary>
internal static class CoreTenantProvisioner
{
    public static async Task EnsureAsync(
        GdcDbContext db,
        Guid tenantId,
        string name,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            db.Tenants.Add(new Tenant
            {
                Id = tenantId,
                Name = name.Trim(),
                IsActive = isActive,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt ?? now,
            });
            return;
        }

        existing.Name = name.Trim();
        existing.IsActive = isActive;
        existing.UpdatedAt = updatedAt ?? now;
    }
}
