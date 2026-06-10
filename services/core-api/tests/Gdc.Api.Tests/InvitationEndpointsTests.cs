using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Gdc.Infrastructure.Tests;

namespace Gdc.Api.Tests;

public class InvitationEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Post_invite_creates_pending_user_and_sends_email()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var token = await LoginAsync(client, AuthTestData.SuperAdminEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/auth/users/invite", new
        {
            email = "invited@example.com",
            tenantId = AuthTestData.TenantBId,
            roleCode = "Operator",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<InviteDto>(JsonOptions);
        Assert.NotNull(body);
        Assert.Equal("Pending", body.Status);
        Assert.Equal(AuthTestData.TenantBId, body.TenantId);

        var emailSender = factory.GetEmailSender();
        Assert.NotNull(emailSender.LastMessage);
    }

    [Fact]
    public async Task Post_invite_returns_409_for_duplicate_email()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var token = await LoginAsync(client, AuthTestData.SuperAdminEmail);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/auth/users/invite", new
        {
            email = AuthTestData.UserBEmail,
            tenantId = AuthTestData.TenantBId,
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_activate_activates_account_with_valid_token()
    {
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(AuthTestData.SeedMultiTenantUsersAsync);
        var client = factory.CreateClient();

        var adminToken = await LoginAsync(client, AuthTestData.SuperAdminEmail);
        var authed = factory.CreateClient();
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var invite = await authed.PostAsJsonAsync("/auth/users/invite", new
        {
            email = "fresh@example.com",
            tenantId = AuthTestData.TenantId,
        });
        invite.EnsureSuccessStatusCode();

        var rawToken = factory.GetActivationTokenFromLastEmail();
        Assert.False(string.IsNullOrWhiteSpace(rawToken));

        var activate = await client.PostAsJsonAsync("/auth/users/activate", new
        {
            token = rawToken,
            password = "FreshPass1",
        });

        Assert.Equal(HttpStatusCode.OK, activate.StatusCode);
        var body = await activate.Content.ReadFromJsonAsync<ActivateDto>(JsonOptions);
        Assert.Equal("Active", body?.Status);

        var login = await client.PostAsJsonAsync("/auth/login", new
        {
            email = "fresh@example.com",
            password = "FreshPass1",
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Post_activate_returns_410_for_expired_token()
    {
        var rawToken = string.Empty;
        await using var factory = new AuthWebApplicationFactory();
        await factory.SeedAsync(async ctx =>
        {
            await AuthTestData.SeedActiveUserAsync(ctx);
            (rawToken, _) = await AuthTestData.SeedPendingUserWithExpiredTokenAsync(ctx);
        });
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/users/activate", new
        {
            token = rawToken,
            password = "FreshPass1",
        });

        Assert.Equal((HttpStatusCode)410, response.StatusCode);
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
    private sealed record InviteDto(Guid UserId, Guid TenantId, string Email, string Status);
    private sealed record ActivateDto(Guid UserId, string Email, string Status);
}
