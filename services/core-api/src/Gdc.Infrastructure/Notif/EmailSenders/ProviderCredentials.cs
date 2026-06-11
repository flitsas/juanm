using System.Text.Json.Serialization;

namespace Gdc.Infrastructure.Notif.EmailSenders;

internal sealed class ApiProviderCredentials
{
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    [JsonPropertyName("validatePath")]
    public string? ValidatePath { get; set; }
}

internal sealed class SendGridProviderCredentials
{
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;
}

internal sealed class FlitMailProviderCredentials
{
    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; } = 587;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("useSsl")]
    public bool UseSsl { get; set; } = true;
}
