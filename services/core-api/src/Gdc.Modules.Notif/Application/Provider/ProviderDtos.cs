using System.Text.Json;

namespace Gdc.Modules.Notif.Application.Provider;

public sealed record ProviderResponse(
    Guid Id,
    string ProviderType,
    string FromAddress,
    bool IsActive,
    bool DispatchEnabled,
    bool HasCredentials,
    DateTimeOffset? UpdatedAt);

public sealed record SaveProviderRequest(
    string ProviderType,
    string FromAddress,
    JsonElement Credentials);

public sealed record TestProviderRequest(
    string Destino,
    string? ProviderType,
    string? FromAddress,
    JsonElement? Credentials);

public sealed record TestProviderResponse(bool Success, string Message);

public sealed class ProviderValidationException(string message) : Exception(message);

public static class ProviderTypes
{
    public const string Api = "api";
    public const string SendGrid = "sendgrid";
    public const string FlitMail = "flit_mail";

    public static bool IsSupported(string? value) =>
        value is Api or SendGrid or FlitMail;
}
