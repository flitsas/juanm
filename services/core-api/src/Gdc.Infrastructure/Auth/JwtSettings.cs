namespace Gdc.Infrastructure.Auth;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "gdc-core-api";

    public string Audience { get; init; } = "gdc-frontend";

    public string SecretKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 15;
}
