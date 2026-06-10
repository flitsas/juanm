using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Auth;

public sealed class AuthSessionService(
    GdcDbContext dbContext,
    IJwtTokenService jwtTokenService) : IAuthSessionService
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    private const string GenericInvalidMessage = "Credenciales inválidas.";

    public async Task<(LoginResponse? Success, AuthError? Error)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var users = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.Email.ToLower() == normalizedEmail)
            .ToListAsync(cancellationToken);

        if (users.Count != 1)
        {
            return (null, new AuthError(AuthErrorCode.InvalidCredentials, GenericInvalidMessage));
        }

        var user = users[0];

        if (user.Status != UserStatus.Active)
        {
            return (null, new AuthError(AuthErrorCode.AccountNotActive, GenericInvalidMessage));
        }

        if (user.LockedUntil is { } lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
        {
            return (null, new AuthError(AuthErrorCode.AccountLocked, GenericInvalidMessage));
        }

        var trackedUser = await dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == user.Id, cancellationToken);

        var verification = PasswordHasher.VerifyHashedPassword(
            trackedUser,
            trackedUser.PasswordHash,
            request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            trackedUser.FailedLoginCount++;
            trackedUser.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return (null, new AuthError(AuthErrorCode.InvalidCredentials, GenericInvalidMessage));
        }

        trackedUser.FailedLoginCount = 0;
        trackedUser.LockedUntil = null;
        trackedUser.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var roleCode = trackedUser.UserRoles
            .Select(ur => ur.Role.Code)
            .OrderBy(code => code)
            .FirstOrDefault() ?? "Operator";

        var (token, expiresAt, _) = jwtTokenService.CreateAccessToken(trackedUser, roleCode);

        return (new LoginResponse(
            token,
            expiresAt,
            trackedUser.Id,
            trackedUser.TenantId,
            trackedUser.Email,
            roleCode), null);
    }

    public async Task LogoutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tokenId = jwtTokenService.GetTokenId(principal);
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return;
        }

        var exists = await dbContext.RevokedTokens
            .AnyAsync(t => t.TokenId == tokenId, cancellationToken);

        if (exists)
        {
            return;
        }

        var expClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Exp);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        if (expClaim is not null && long.TryParse(expClaim, out var unixExp))
        {
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(unixExp);
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.RevokedTokens.Add(new RevokedToken
        {
            Id = Guid.NewGuid(),
            TokenId = tokenId,
            ExpiresAt = expiresAt,
            RevokedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsTokenRevokedAsync(string tokenId, CancellationToken cancellationToken = default) =>
        dbContext.RevokedTokens.AsNoTracking().AnyAsync(t => t.TokenId == tokenId, cancellationToken);
}
