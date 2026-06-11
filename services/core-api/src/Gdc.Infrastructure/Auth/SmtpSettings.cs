using Microsoft.Extensions.Configuration;

namespace Gdc.Infrastructure.Auth;

public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public const string EnvEnabled = "SMTP_ENABLED";
    public const string EnvHost = "SMTP_HOST";
    public const string EnvPort = "SMTP_PORT";
    public const string EnvUser = "SMTP_USER";
    public const string EnvPassword = "SMTP_PASSWORD";
    public const string EnvFrom = "SMTP_FROM";
    public const string EnvFromName = "SMTP_FROM_NAME";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string FromDisplayName { get; set; } = "GDC 2.0";

    public static void Bind(IConfiguration configuration, SmtpSettings settings)
    {
        settings.Host = FirstNonEmpty(
            Environment.GetEnvironmentVariable(EnvHost),
            configuration[$"{SectionName}:Host"]) ?? string.Empty;

        settings.Port = ParseInt(
            Environment.GetEnvironmentVariable(EnvPort),
            configuration.GetValue<int?>($"{SectionName}:Port"),
            587);

        settings.Username = FirstNonEmpty(
            Environment.GetEnvironmentVariable(EnvUser),
            configuration[$"{SectionName}:Username"]) ?? string.Empty;

        settings.Password = FirstNonEmpty(
            Unquote(Environment.GetEnvironmentVariable(EnvPassword)),
            configuration[$"{SectionName}:Password"]) ?? string.Empty;

        settings.FromAddress = FirstNonEmpty(
            Environment.GetEnvironmentVariable(EnvFrom),
            configuration[$"{SectionName}:FromAddress"]) ?? string.Empty;

        settings.FromDisplayName = FirstNonEmpty(
            Environment.GetEnvironmentVariable(EnvFromName),
            configuration[$"{SectionName}:FromDisplayName"]) ?? "GDC 2.0";

        settings.EnableSsl = ParseBool(
            Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL"),
            configuration.GetValue<bool?>($"{SectionName}:EnableSsl"),
            true);

        var enabledFromEnv = Environment.GetEnvironmentVariable(EnvEnabled);
        if (enabledFromEnv is not null && bool.TryParse(enabledFromEnv, out var enabled))
        {
            settings.Enabled = enabled;
        }
        else
        {
            settings.Enabled = configuration.GetValue<bool?>($"{SectionName}:Enabled")
                ?? !string.IsNullOrWhiteSpace(settings.Host);
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static int ParseInt(string? envValue, int? configValue, int defaultValue)
    {
        if (!string.IsNullOrWhiteSpace(envValue) && int.TryParse(envValue, out var parsed))
        {
            return parsed;
        }

        return configValue ?? defaultValue;
    }

    private static bool ParseBool(string? envValue, bool? configValue, bool defaultValue)
    {
        if (!string.IsNullOrWhiteSpace(envValue) && bool.TryParse(envValue, out var parsed))
        {
            return parsed;
        }

        return configValue ?? defaultValue;
    }

    private static string? Unquote(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var trimmed = value.Trim();
        if (trimmed.Length >= 2
            && ((trimmed.StartsWith('\'') && trimmed.EndsWith('\''))
                || (trimmed.StartsWith('"') && trimmed.EndsWith('"'))))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }
}
