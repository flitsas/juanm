namespace Gdc.Infrastructure.Auth;

public sealed class LockoutSettings
{
    public const string SectionName = "Auth:Lockout";

    public int MaxFailedAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
