using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gdc.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTimeOffset ExpiresAt, string TokenId) CreateAccessToken(User user, string roleCode)
    {
        var tokenId = Guid.NewGuid().ToString("N");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new("tenant_id", user.TenantId.ToString()),
            new(ClaimTypes.Role, roleCode),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return (encoded, expiresAt, tokenId);
    }

    public string? GetTokenId(ClaimsPrincipal principal) =>
        principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
}
