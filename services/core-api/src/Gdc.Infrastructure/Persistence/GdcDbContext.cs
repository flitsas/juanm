using Gdc.Infrastructure.Persistence.Auth;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Persistence;

public sealed class GdcDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public GdcDbContext(DbContextOptions<GdcDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ActivationToken> ActivationTokens => Set<ActivationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GdcDbContext).Assembly);
        AuthSeedData.SeedRoles(modelBuilder);
        AuthSeedData.SeedPermissions(modelBuilder);
        AuthSeedData.SeedDefaultRolePermissions(modelBuilder);

        modelBuilder.Entity<User>().HasQueryFilter(user =>
            user.DeletedAt == null &&
            (_tenantContext.BypassTenantFilter ||
             (_tenantContext.TenantId != null && user.TenantId == _tenantContext.TenantId)));

        base.OnModelCreating(modelBuilder);
    }
}
