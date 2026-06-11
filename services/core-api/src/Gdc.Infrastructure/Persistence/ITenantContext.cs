namespace Gdc.Infrastructure.Persistence;

public interface ITenantContext
{
    Guid? TenantId { get; }

    bool IsSuperAdmin { get; }

    bool BypassTenantFilter { get; }

    void SetFromClaims(Guid? tenantId, string? roleCode);

    void EnableBypass();
}
