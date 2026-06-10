namespace Gdc.Infrastructure.Persistence.Auth.Entities;

public enum UserStatus
{
    Pending = 0,
    Active = 1,
    Locked = 2,
}

public sealed class User : AuditableEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Pending;

    public int FailedLoginCount { get; set; }

    public DateTimeOffset? LockedUntil { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
