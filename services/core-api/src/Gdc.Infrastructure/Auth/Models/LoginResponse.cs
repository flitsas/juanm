namespace Gdc.Infrastructure.Auth.Models;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    Guid TenantId,
    string Email,
    string Role);
