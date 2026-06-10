using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Tests;

/// <summary>
/// Uso de ejemplo: solicitar recuperación de contraseña y resetear con token.
/// </summary>
public class PasswordRecoveryServiceTests
{
    private static PasswordRecoveryService CreateService(
        GdcDbContext context,
        CapturingEmailSender emailSender) =>
        new(context, emailSender, PasswordRecoveryTestSettings.Options);

    [Fact]
    public async Task ForgotPasswordAsync_always_accepts_request()
    {
        var emailSender = new CapturingEmailSender();
        await using var context = TestDbContextFactory.CreateInMemory();
        await AuthTestData.SeedActiveUserAsync(context);
        var service = CreateService(context, emailSender);

        var (success, error) = await service.ForgotPasswordAsync(
            new ForgotPasswordRequest("unknown@example.com"));

        Assert.Null(error);
        Assert.True(success!.Accepted);
        Assert.Null(emailSender.LastMessage);
    }

    [Fact]
    public async Task ForgotPasswordAsync_sends_email_for_registered_user()
    {
        var emailSender = new CapturingEmailSender();
        await using var context = TestDbContextFactory.CreateInMemory();
        await AuthTestData.SeedActiveUserAsync(context);
        var service = CreateService(context, emailSender);

        var (success, error) = await service.ForgotPasswordAsync(
            new ForgotPasswordRequest(AuthTestData.Email));

        Assert.Null(error);
        Assert.True(success!.Accepted);
        Assert.NotNull(emailSender.LastMessage);
        Assert.Contains("token=", emailSender.LastMessage!.Body);
    }

    [Fact]
    public async Task ResetPasswordAsync_updates_password_and_consumes_token()
    {
        var emailSender = new CapturingEmailSender();
        await using var context = TestDbContextFactory.CreateInMemory();
        await AuthTestData.SeedActiveUserAsync(context);
        var service = CreateService(context, emailSender);

        await service.ForgotPasswordAsync(new ForgotPasswordRequest(AuthTestData.Email));
        var rawToken = ExtractToken(emailSender.LastMessage!.Body);

        var (success, error) = await service.ResetPasswordAsync(
            new ResetPasswordRequest(rawToken, "NewPass2!"));

        Assert.Null(error);
        Assert.NotNull(success);
        Assert.Equal(AuthTestData.Email, success!.Email);

        var tokenRow = await context.PasswordResetTokens
            .IgnoreQueryFilters()
            .FirstAsync();
        Assert.NotNull(tokenRow.UsedAt);
    }

    [Fact]
    public async Task AdminSetPasswordAsync_updates_active_user_password()
    {
        var emailSender = new CapturingEmailSender();
        var tenantContext = new TenantContext();
        tenantContext.SetFromClaims(AuthTestData.TenantId, AuthRoles.SuperAdmin);
        await using var context = TestDbContextFactory.CreateInMemory(tenantContext);
        await AuthTestData.SeedActiveUserAsync(context);
        var service = CreateService(context, emailSender);

        var (success, error) = await service.AdminSetPasswordAsync(
            AuthTestData.UserId,
            new AdminSetPasswordRequest("AdminSet1!"));

        Assert.Null(error);
        Assert.NotNull(success);
        Assert.Equal(AuthTestData.UserId, success!.UserId);
    }

    [Fact]
    public async Task LoginAsync_locks_account_after_failed_threshold_with_suggestion()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        await AuthTestData.SeedActiveUserAsync(context);
        var session = new AuthSessionService(context, PasswordRecoveryTestSettings.Jwt, PasswordRecoveryTestSettings.Lockout);

        for (var i = 0; i < 5; i++)
        {
            await session.LoginAsync(new LoginRequest(AuthTestData.Email, "wrong"));
        }

        var (_, error) = await session.LoginAsync(new LoginRequest(AuthTestData.Email, "wrong"));

        Assert.NotNull(error);
        Assert.Equal(AuthErrorCode.AccountLocked, error!.Code);
        Assert.Contains("recuperar", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractToken(string body)
    {
        const string marker = "token=";
        var start = body.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return body[start..];
    }
}

internal static class PasswordRecoveryTestSettings
{
    public static readonly Microsoft.Extensions.Options.IOptions<LockoutSettings> Lockout =
        Microsoft.Extensions.Options.Options.Create(new LockoutSettings
        {
            MaxFailedAttempts = 5,
            LockoutMinutes = 15,
        });

    public static readonly Microsoft.Extensions.Options.IOptions<PasswordRecoverySettings> Options =
        Microsoft.Extensions.Options.Options.Create(new PasswordRecoverySettings
        {
            ResetTokenHours = 48,
            ResetBaseUrl = "http://localhost:4001/reset",
        });

    public static readonly IJwtTokenService Jwt = new JwtTokenService(
        Microsoft.Extensions.Options.Options.Create(new JwtSettings
        {
            SecretKey = "test-secret-key-with-at-least-32-characters",
            Issuer = "test",
            Audience = "test",
            AccessTokenMinutes = 15,
        }));
}
