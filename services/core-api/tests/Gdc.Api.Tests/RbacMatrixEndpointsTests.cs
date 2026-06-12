using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Gdc.Infrastructure.Persistence.Auth;
using Gdc.Infrastructure.Tests;

namespace Gdc.Api.Tests;

public class RbacMatrixEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Get_rbac_matrix_returns_roles_and_permissions_for_super_admin()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = await CreateSuperAdminClientAsync(factory);

        var response = await client.GetAsync("/api/v1/auth/rbac/matrix");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var matrix = await response.Content.ReadFromJsonAsync<MatrixDto>(JsonOptions);
        Assert.NotNull(matrix);
        Assert.True(matrix!.Permissions.Count >= 3);
        Assert.True(matrix.Roles.Count >= 3);
    }

    [Fact]
    public async Task Put_rbac_matrix_persists_changes_for_super_admin()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = await CreateSuperAdminClientAsync(factory);

        var update = await client.PutAsJsonAsync("/api/v1/auth/rbac/matrix", new
        {
            assignments = new[]
            {
                new { roleId = AuthRoleIds.Operator, permissionId = AuthPermissionIds.UsersWrite, enabled = true },
            },
        });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var get = await client.GetAsync("/api/v1/auth/rbac/matrix");
        var matrix = await get.Content.ReadFromJsonAsync<MatrixDto>(JsonOptions);
        var op = matrix!.Roles.Single(r => r.Code == "Operator");
        Assert.Contains(AuthPermissionIds.UsersWrite, op.PermissionIds);
    }

    [Fact]
    public async Task Put_rbac_matrix_returns_403_for_non_super_admin()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var token = await LoginAsync(client, AuthTestData.Email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync("/api/v1/auth/rbac/matrix", new
        {
            assignments = new[]
            {
                new { roleId = AuthRoleIds.Operator, permissionId = AuthPermissionIds.UsersWrite, enabled = true },
            },
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_rbac_matrix_returns_403_for_non_super_admin()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var token = await LoginAsync(client, AuthTestData.Email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/auth/rbac/matrix");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<HttpClient> CreateSuperAdminClientAsync(AuthWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var token = await LoginAsync(client, AuthTestData.SuperAdminEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = AuthTestData.Password,
        });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<LoginDto>(JsonOptions);
        return body!.AccessToken;
    }

    private sealed record LoginDto(string AccessToken);

    private sealed record MatrixDto(
        List<PermissionDto> Permissions,
        List<RoleDto> Roles);

    private sealed record PermissionDto(Guid Id, string Code, string Module, string Action, string? Description);

    private sealed record RoleDto(Guid RoleId, string Code, string Name, List<Guid> PermissionIds);
}
