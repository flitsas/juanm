using System.Security.Claims;
using Gdc.Infrastructure.Persistence.Auth.Entities;

namespace Gdc.Infrastructure.Auth;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt, string TokenId) CreateAccessToken(User user, string roleCode);

    string? GetTokenId(ClaimsPrincipal principal);
}
