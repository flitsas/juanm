namespace Gdc.Infrastructure.Persistence.Auth.Entities;

public sealed class Role : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
