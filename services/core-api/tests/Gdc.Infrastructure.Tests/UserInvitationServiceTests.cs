using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Tests;

/// <summary>
/// Uso de ejemplo: invitar usuario pending y activar con token de 48h.
/// </summary>
public class UserInvitationServiceTests
{
    private static UserInvitationService CreateService(
        GdcDbContext context,
        CapturingEmailSender emailSender,
        TenantContext? tenantContext = null)
    {
        var tenant = tenantContext ?? CreateSuperAdminContext();
        return new UserInvitationService(
            context,
            tenant,
            emailSender,
            Options.Create(new InvitationSettings
            {
                ActivationTokenHours = 48,
                ActivationBaseUrl = "http://localhost:40103/activate",
            }),
            NullLogger<UserInvitationService>.Instance);
    }

    private static TenantContext CreateSuperAdminContext()
    {
        var ctx = new TenantContext();
        ctx.SetFromClaims(AuthTestData.TenantId, AuthRoles.SuperAdmin);
        return ctx;
    }

    [Fact]
    public async Task InviteAsync_creates_pending_user_and_sends_email()
    {
        var emailSender = new CapturingEmailSender();
        var tenantContext = CreateSuperAdminContext();
        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedActiveUserAsync(context);

        var service = CreateService(context, emailSender, tenantContext);
        var (success, error) = await service.InviteAsync(
            new InviteUserRequest("new.user@example.com", AuthTestData.TenantId));

        Assert.Null(error);
        Assert.NotNull(success);
        Assert.Equal("Pending", success.Status);
        Assert.NotNull(emailSender.LastMessage);
        Assert.Contains("token=", emailSender.LastMessage.Body);

        var stored = await context.Users
            .IgnoreQueryFilters()
            .FirstAsync(u => u.Id == success.UserId);
        Assert.Equal(UserStatus.Pending, stored.Status);
    }

    [Fact]
    public async Task InviteAsync_returns_conflict_for_duplicate_email()
    {
        var emailSender = new CapturingEmailSender();
        var tenantContext = CreateSuperAdminContext();
        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedActiveUserAsync(context);

        var service = CreateService(context, emailSender, tenantContext);
        var (_, error) = await service.InviteAsync(
            new InviteUserRequest(AuthTestData.Email, AuthTestData.TenantId));

        Assert.NotNull(error);
        Assert.Equal(InvitationErrorCode.DuplicateEmail, error.Code);
    }

    [Fact]
    public async Task ActivateAsync_activates_user_and_consumes_token()
    {
        var emailSender = new CapturingEmailSender();
        var tenantContext = CreateSuperAdminContext();
        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedActiveUserAsync(context);

        var service = CreateService(context, emailSender, tenantContext);
        var (invite, _) = await service.InviteAsync(
            new InviteUserRequest("activate.me@example.com", AuthTestData.TenantId));
        Assert.NotNull(invite);

        var rawToken = ExtractTokenFromEmail(emailSender.LastMessage!.Body);
        var (activated, error) = await service.ActivateAsync(
            new ActivateUserRequest(rawToken, "NewPass1!"));

        Assert.Null(error);
        Assert.NotNull(activated);
        Assert.Equal("Active", activated.Status);

        var tokenRow = await context.ActivationTokens
            .IgnoreQueryFilters()
            .FirstAsync(t => t.UserId == invite.UserId);
        Assert.NotNull(tokenRow.UsedAt);
    }

    [Fact]
    public async Task InviteAsync_includes_activation_base_url_in_email_body()
    {
        var emailSender = new CapturingEmailSender();
        var tenantContext = CreateSuperAdminContext();
        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedActiveUserAsync(context);

        var service = CreateService(context, emailSender, tenantContext);
        var (_, error) = await service.InviteAsync(
            new InviteUserRequest("link.user@example.com", AuthTestData.TenantId));

        Assert.Null(error);
        Assert.NotNull(emailSender.LastMessage);
        Assert.Contains("http://localhost:40103/activate?token=", emailSender.LastMessage.Body);
        Assert.Contains("48 horas", emailSender.LastMessage.Body);
    }

    [Fact]
    public async Task ActivateAsync_returns_expired_for_token_older_than_48h()
    {
        var emailSender = new CapturingEmailSender();
        await using var context = TestDbContextFactory.CreateInMemory(CreateSuperAdminContext());
        await AuthTestData.SeedActiveUserAsync(context);

        var (rawToken, _) = await AuthTestData.SeedPendingUserWithExpiredTokenAsync(context);

        var service = CreateService(context, emailSender);
        var (_, error) = await service.ActivateAsync(
            new ActivateUserRequest(rawToken, "NewPass1!"));

        Assert.Equal(InvitationErrorCode.ExpiredToken, error!.Code);
    }

    private static string ExtractTokenFromEmail(string body)
    {
        const string marker = "token=";
        var start = body.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return body[start..];
    }
}
