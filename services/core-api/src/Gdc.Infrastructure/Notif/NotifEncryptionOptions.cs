namespace Gdc.Infrastructure.Notif;

public sealed class NotifEncryptionOptions
{
    public const string SectionName = "Notif:Encryption";

    public string Key { get; set; } = string.Empty;
}
