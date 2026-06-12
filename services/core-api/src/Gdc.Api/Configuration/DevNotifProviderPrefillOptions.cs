namespace Gdc.Api.Configuration;

/// <summary>
/// Precarga local (solo Development) para el formulario NOTIF en frontend.
/// Definir en appsettings.Development.local.json — nunca commitear credenciales.
/// </summary>
public sealed class DevNotifProviderPrefillOptions
{
    public const string SectionName = "DevNotifProviderPrefill";

    public string ProviderType { get; set; } = "flit_mail";

    public string FromAddress { get; set; } = string.Empty;

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public string SmtpUsername { get; set; } = string.Empty;

    public string SmtpPassword { get; set; } = string.Empty;

    public bool SmtpUseSsl { get; set; } = true;

    public string TestDestino { get; set; } = string.Empty;
}
