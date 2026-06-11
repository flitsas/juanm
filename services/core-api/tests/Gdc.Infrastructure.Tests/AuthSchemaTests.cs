using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Tests;

public class AuthSchemaTests
{
    [Fact]
    public void Model_includes_auth_tables_with_tenant_scoped_users()
    {
        using var context = CreateContext();

        var entityTypes = context.Model.GetEntityTypes()
            .Select(e => (Schema: e.GetSchema() ?? "public", Table: e.GetTableName()))
            .ToList();

        Assert.Contains(("auth", "users"), entityTypes);
        Assert.Contains(("auth", "roles"), entityTypes);
        Assert.Contains(("auth", "permissions"), entityTypes);
        Assert.Contains(("auth", "role_permissions"), entityTypes);
        Assert.Contains(("auth", "user_roles"), entityTypes);
        Assert.Contains(("auth", "activation_tokens"), entityTypes);
        Assert.Contains(("auth", "password_reset_tokens"), entityTypes);
        Assert.Contains(("auth", "revoked_tokens"), entityTypes);
        Assert.Contains(("core", "tenants"), entityTypes);

        var userEntity = context.Model.FindEntityType(typeof(User));
        Assert.NotNull(userEntity);
        Assert.True(userEntity.FindProperty(nameof(User.TenantId))?.IsNullable == false);
    }

    [Fact]
    public void Seed_roles_include_SuperAdmin_TenantAdmin_Operator()
    {
        using var context = CreateContext();

        var roles = context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Code)
            .Select(r => r.Code)
            .ToList();

        Assert.Equal(["Operator", "SuperAdmin", "TenantAdmin"], roles);
        Assert.All(AuthRoleIds.All, id => Assert.Contains(id, context.Roles.Select(r => r.Id)));
    }

    [Fact]
    public void RowVersion_has_database_default_for_inserts()
    {
        using var context = CreateContext();

        var userEntity = context.Model.FindEntityType(typeof(User));
        Assert.NotNull(userEntity);

        var rowVersion = userEntity.FindProperty(nameof(AuditableEntity.RowVersion));
        Assert.NotNull(rowVersion);
        Assert.Equal("'0'::xid", rowVersion.GetDefaultValueSql());
        Assert.True(rowVersion.IsConcurrencyToken);
    }

    [Fact]
    public void AddAuthSchema_migration_is_registered()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseNpgsql("Host=localhost;Database=gdc_test;Username=postgres;Password=postgres")
            .Options;

        using var context = new GdcDbContext(options, new TenantContext());
        var migrations = context.Database.GetMigrations().ToList();

        Assert.Contains(migrations, m => m.EndsWith("_AddAuthSchema", StringComparison.Ordinal));
    }

    private static GdcDbContext CreateContext() => TestDbContextFactory.CreateInMemory();
}
