using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Gdc.Infrastructure.Tests;

namespace Gdc.Api.Tests;

public class AuthEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Post_login_returns_200_with_jwt_for_valid_credentials()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthTestData.Email,
            password = AuthTestData.Password,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
        Assert.Equal(AuthTestData.TenantId, body.TenantId);
        Assert.Equal("Operator", body.Role);
    }

    [Fact]
    public async Task Post_login_returns_401_for_invalid_password()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthTestData.Email,
            password = "invalid",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MessageDto>(JsonOptions);
        Assert.Equal("Credenciales inválidas.", body?.Message);
    }

    [Fact]
    public async Task Post_logout_returns_401_when_token_revoked_on_reuse()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthTestData.Email,
            password = AuthTestData.Password,
        });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginDto>(JsonOptions);
        Assert.NotNull(loginBody);

        var authed = factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);

        var logout = await authed.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var reuse = await authed.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Post_refresh_returns_new_tokens_for_valid_refresh_token()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthTestData.Email,
            password = AuthTestData.Password,
        });
        var loginBody = await login.Content.ReadFromJsonAsync<LoginDto>(JsonOptions);
        Assert.NotNull(loginBody);

        var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = loginBody.RefreshToken,
        });

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var refreshBody = await refresh.Content.ReadFromJsonAsync<LoginDto>(JsonOptions);
        Assert.NotNull(refreshBody);
        Assert.NotEqual(loginBody.AccessToken, refreshBody.AccessToken);
        Assert.NotEqual(loginBody.RefreshToken, refreshBody.RefreshToken);
    }

    private sealed record LoginDto(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAt,
        Guid UserId,
        Guid TenantId,
        string Email,
        string Role);

    private sealed record MessageDto(string Message);
}
