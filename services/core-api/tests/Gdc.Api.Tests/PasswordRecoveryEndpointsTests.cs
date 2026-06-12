using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Gdc.Infrastructure.Tests;

namespace Gdc.Api.Tests;

public class PasswordRecoveryEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Post_forgot_password_returns_202_for_unknown_email()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/password/forgot", new
        {
            email = "not-registered@example.com",
        });

        Assert.Equal((HttpStatusCode)202, response.StatusCode);
    }

    [Fact]
    public async Task Post_forgot_password_returns_202_for_registered_email()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/password/forgot", new
        {
            email = AuthTestData.Email,
        });

        Assert.Equal((HttpStatusCode)202, response.StatusCode);
        Assert.NotNull(factory.GetEmailSender().LastMessage);
    }

    [Fact]
    public async Task Post_password_reset_allows_login_with_new_password()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/v1/auth/password/forgot", new { email = AuthTestData.Email });
        var token = factory.GetPasswordResetTokenFromLastEmail();

        var reset = await client.PostAsJsonAsync("/api/v1/auth/password/reset", new
        {
            token,
            password = "ResetPass1",
        });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthTestData.Email,
            password = "ResetPass1",
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Put_user_password_by_admin_allows_login_with_new_password()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var adminToken = await LoginAsync(client, AuthTestData.SuperAdminEmail);
        var authed = factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await authed.PutAsJsonAsync(
            $"/api/v1/auth/users/{AuthTestData.UserId}/password",
            new { password = "AdminNew1!" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthTestData.Email,
            password = "AdminNew1!",
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Post_login_returns_locked_suggestion_after_threshold()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedActiveUserAsync);
        var client = factory.CreateClient();

        for (var i = 0; i < 6; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = AuthTestData.Email,
                password = "wrong",
            });
        }

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthTestData.Email,
            password = "wrong",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MessageDto>(JsonOptions);
        Assert.Contains("recuperar", body?.Message ?? "", StringComparison.OrdinalIgnoreCase);
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
    private sealed record MessageDto(string Message);
}
