using Gdc.Infrastructure.Persistence;

namespace Gdc.Api.Tests.TestSupport;

/// <summary>
/// <see cref="ITenantContext"/> para tests: omite el filtro multi-tenant (super admin con
/// bypass) para que las pruebas en memoria vean todo lo que siembran. El unico filtro que
/// depende del tenant es el de <c>User</c>; el resto de entidades se rige por soft-delete.
/// </summary>
internal sealed class TestTenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public bool IsSuperAdmin { get; private set; } = true;

    public bool BypassTenantFilter { get; private set; } = true;

    public void SetFromClaims(Guid? tenantId, string? roleCode)
    {
        TenantId = tenantId;
        IsSuperAdmin = string.Equals(roleCode, "super_admin", StringComparison.Ordinal);
        BypassTenantFilter = false;
    }

    public void ApplyHeaderTenant(Guid headerTenantId) => TenantId = headerTenantId;

    public void EnableBypass() => BypassTenantFilter = true;
}
