namespace Gdc.Infrastructure.Auth;

public sealed class PasswordRecoverySettings
{
    public const string SectionName = "Auth:PasswordRecovery";

    public int ResetTokenHours { get; set; } = 48;

    public string ResetBaseUrl { get; set; } = "http://localhost:40103/reset";
}
