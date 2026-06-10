namespace Gdc.Infrastructure.Auth;

public sealed class InvitationSettings
{
    public const string SectionName = "Auth:Invitation";

    public int ActivationTokenHours { get; set; } = 48;

    public string ActivationBaseUrl { get; set; } = "http://localhost:4001/activate";
}
