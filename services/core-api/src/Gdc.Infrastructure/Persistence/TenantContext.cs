using Gdc.Infrastructure.Auth;

namespace Gdc.Infrastructure.Persistence;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public bool IsSuperAdmin { get; private set; }

    public bool BypassTenantFilter { get; private set; }

    public void SetFromClaims(Guid? tenantId, string? roleCode)
    {
        TenantId = tenantId;
        IsSuperAdmin = string.Equals(roleCode, AuthRoles.SuperAdmin, StringComparison.Ordinal);
        BypassTenantFilter = false;
    }

    public void EnableBypass()
    {
        if (!IsSuperAdmin)
        {
            throw new InvalidOperationException("Bypass de tenant solo permitido para SuperAdmin.");
        }

        BypassTenantFilter = true;
    }
}
