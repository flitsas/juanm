using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Infrastructure.Notif.EmailSenders;

internal sealed class SendGridEmailSender(HttpClient httpClient, string credentialsJson) : IEmailSender
{
    private readonly SendGridProviderCredentials _credentials = JsonSerializer.Deserialize<SendGridProviderCredentials>(credentialsJson)
        ?? throw new ArgumentException("Invalid SendGrid provider credentials.");

    public async Task ValidateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_credentials.ApiKey))
        {
            throw new InvalidOperationException("SendGrid apiKey is required.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.sendgrid.com/v3/scopes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"SendGrid validation failed with status {(int)response.StatusCode}.");
        }
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_credentials.ApiKey))
        {
            return new EmailSendResult(false, null, "SendGrid apiKey is required.");
        }

        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = message.To } } } },
            from = new { email = message.From },
            subject = message.Subject,
            content = new[] { new { type = "text/html", value = message.HtmlBody } },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new EmailSendResult(false, null, $"SendGrid send failed: {(int)response.StatusCode} {body}");
        }

        var messageId = response.Headers.TryGetValues("X-Message-Id", out var values)
            ? values.FirstOrDefault()
            : null;

        return new EmailSendResult(true, messageId, null);
    }
}
