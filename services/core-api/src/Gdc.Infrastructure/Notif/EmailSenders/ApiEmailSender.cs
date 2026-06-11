using System.Net.Http.Headers;
using System.Text.Json;
using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Infrastructure.Notif.EmailSenders;

internal sealed class ApiEmailSender(HttpClient httpClient, string credentialsJson) : IEmailSender
{
    private readonly ApiProviderCredentials _credentials = JsonSerializer.Deserialize<ApiProviderCredentials>(credentialsJson)
        ?? throw new ArgumentException("Invalid API provider credentials.");

    public async Task ValidateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_credentials.BaseUrl))
        {
            throw new InvalidOperationException("API baseUrl is required.");
        }

        var path = string.IsNullOrWhiteSpace(_credentials.ValidatePath) ? "/" : _credentials.ValidatePath;
        var request = new HttpRequestMessage(HttpMethod.Get, CombineUrl(_credentials.BaseUrl, path));
        if (!string.IsNullOrWhiteSpace(_credentials.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.ApiKey);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"API provider validation failed with status {(int)response.StatusCode}.");
        }
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        await ValidateAsync(cancellationToken);
        return new EmailSendResult(true, Guid.NewGuid().ToString("N"), null);
    }

    private static string CombineUrl(string baseUrl, string path)
    {
        var trimmed = baseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : "/" + path;
        return trimmed + normalizedPath;
    }
}
