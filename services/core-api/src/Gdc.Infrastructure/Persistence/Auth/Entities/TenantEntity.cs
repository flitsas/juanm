namespace Gdc.Infrastructure.Persistence.Auth.Entities;

/// <summary>Stub de tenant en schema core hasta módulo tenants dedicado.</summary>
public sealed class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
