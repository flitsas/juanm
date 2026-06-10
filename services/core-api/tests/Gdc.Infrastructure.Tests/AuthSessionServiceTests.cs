using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Tests;

public class AuthSessionServiceTests
{
    private static AuthSessionService CreateService(GdcDbContext context)
    {
        var jwt = new JwtTokenService(Options.Create(new JwtSettings
        {
            SecretKey = "test-secret-key-with-at-least-32-characters",
            Issuer = "test",
            Audience = "test",
            AccessTokenMinutes = 15,
        }));

        return new AuthSessionService(context, jwt);
    }

    [Fact]
    public async Task LoginAsync_with_valid_credentials_returns_jwt_with_tenant_and_role()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        await AuthTestData.SeedActiveUserAsync(context);
        var service = CreateService(context);

        var (success, error) = await service.LoginAsync(
            new LoginRequest(AuthTestData.Email, AuthTestData.Password));

        Assert.Null(error);
        Assert.NotNull(success);
        Assert.False(string.IsNullOrWhiteSpace(success.AccessToken));
        Assert.Equal(AuthTestData.UserId, success.UserId);
        Assert.Equal(AuthTestData.TenantId, success.TenantId);
        Assert.Equal("Operator", success.Role);
    }

    [Fact]
    public async Task LoginAsync_with_invalid_password_returns_generic_error()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        await AuthTestData.SeedActiveUserAsync(context);
        var service = CreateService(context);

        var (success, error) = await service.LoginAsync(
            new LoginRequest(AuthTestData.Email, "wrong-password"));

        Assert.Null(success);
        Assert.NotNull(error);
        Assert.Equal(AuthErrorCode.InvalidCredentials, error.Code);
        Assert.Equal("Credenciales inválidas.", error.Message);
    }

    [Fact]
    public async Task LogoutAsync_revokes_token_and_blocks_reuse()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        await AuthTestData.SeedActiveUserAsync(context);
        var jwt = new JwtTokenService(Options.Create(new JwtSettings
        {
            SecretKey = "test-secret-key-with-at-least-32-characters",
            Issuer = "test",
            Audience = "test",
            AccessTokenMinutes = 15,
        }));
        var service = CreateService(context);

        var user = await context.Users
            .IgnoreQueryFilters()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync();

        var (token, _, tokenId) = jwt.CreateAccessToken(user, "Operator");
        Assert.False(string.IsNullOrWhiteSpace(token));

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(
            token,
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "test",
                ValidAudience = "test",
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes("test-secret-key-with-at-least-32-characters")),
            },
            out _);

        await service.LogoutAsync(principal);
        Assert.True(await service.IsTokenRevokedAsync(tokenId));
    }
}
