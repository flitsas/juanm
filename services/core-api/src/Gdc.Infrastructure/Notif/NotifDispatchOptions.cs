namespace Gdc.Infrastructure.Notif;

public sealed class NotifDispatchOptions
{
    public const string SectionName = "Notif:Dispatch";

    public bool Enabled { get; set; } = true;

    public int PollIntervalSeconds { get; set; } = 30;
}
