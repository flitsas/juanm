using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Persistence;

namespace Gdc.Infrastructure.Tests;

public class MultiTenantFilterTests
{
    [Fact]
    public async Task ListCurrentTenantUsers_returns_only_matching_tenant()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetFromClaims(AuthTestData.TenantId, AuthRoles.Operator);

        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedMultiTenantUsersAsync(context);

        var service = new UserReadService(context, tenantContext);
        var users = await service.ListCurrentTenantUsersAsync();

        Assert.All(users, u => Assert.Equal(AuthTestData.TenantId, u.TenantId));
        Assert.Contains(users, u => u.Id == AuthTestData.UserId);
        Assert.DoesNotContain(users, u => u.Id == AuthTestData.UserBId);
    }

    [Fact]
    public async Task GetUserById_returns_null_for_cross_tenant_user()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetFromClaims(AuthTestData.TenantId, AuthRoles.Operator);

        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedMultiTenantUsersAsync(context);

        var service = new UserReadService(context, tenantContext);
        var user = await service.GetUserByIdAsync(AuthTestData.UserBId);

        Assert.Null(user);
    }

    [Fact]
    public async Task SuperAdmin_can_list_all_users_with_bypass()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetFromClaims(AuthTestData.TenantId, AuthRoles.SuperAdmin);

        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedMultiTenantUsersAsync(context);

        var service = new UserReadService(context, tenantContext);
        var users = await service.ListAllUsersForSuperAdminAsync();

        Assert.True(users.Count >= 3);
        Assert.Contains(users, u => u.Id == AuthTestData.UserBId);
    }
}
