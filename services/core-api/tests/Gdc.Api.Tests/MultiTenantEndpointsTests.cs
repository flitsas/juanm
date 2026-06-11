using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Gdc.Infrastructure.Tests;

namespace Gdc.Api.Tests;

public class MultiTenantEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Get_users_returns_only_current_tenant_records()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var token = await LoginAsync(client, AuthTestData.Email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/auth/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(JsonOptions);
        Assert.NotNull(users);
        Assert.All(users!, u => Assert.Equal(AuthTestData.TenantId, u.TenantId));
        Assert.DoesNotContain(users!, u => u.Id == AuthTestData.UserBId);
    }

    [Fact]
    public async Task Get_user_by_id_returns_not_found_for_other_tenant()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var token = await LoginAsync(client, AuthTestData.Email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/auth/users/{AuthTestData.UserBId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_admin_users_returns_cross_tenant_list()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var token = await LoginAsync(client, AuthTestData.SuperAdminEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/auth/admin/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(JsonOptions);
        Assert.NotNull(users);
        Assert.Contains(users!, u => u.Id == AuthTestData.UserBId);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var login = await client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = AuthTestData.Password,
        });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<LoginDto>(JsonOptions);
        return body!.AccessToken;
    }

    private sealed record LoginDto(string AccessToken);
    private sealed record UserDto(Guid Id, Guid TenantId, string Email, string Status);
}
