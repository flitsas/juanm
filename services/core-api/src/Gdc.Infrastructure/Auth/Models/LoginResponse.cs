namespace Gdc.Infrastructure.Auth.Models;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    Guid TenantId,
    string Email,
    string Role);
