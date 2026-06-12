using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text.Json;
using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Infrastructure.Notif.EmailSenders;

internal sealed class FlitMailEmailSender(string credentialsJson) : IEmailSender
{
    private readonly FlitMailProviderCredentials _credentials = JsonSerializer.Deserialize<FlitMailProviderCredentials>(credentialsJson)
        ?? throw new ArgumentException("Invalid FLIT Mail provider credentials.");

    public async Task ValidateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_credentials.Host))
        {
            throw new InvalidOperationException("FLIT Mail host is required.");
        }

        if (string.IsNullOrWhiteSpace(_credentials.Username))
        {
            throw new InvalidOperationException("FLIT Mail username is required.");
        }

        using var tcp = new TcpClient();
        await tcp.ConnectAsync(_credentials.Host, _credentials.Port, cancellationToken);
    }

    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        using var mail = new MailMessage(message.From, message.To, message.Subject, message.HtmlBody) { IsBodyHtml = true };
        if (message.Attachments is not null)
        {
            foreach (var attachment in message.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                mail.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
            }
        }

        client.Send(mail);
        return Task.FromResult(new EmailSendResult(true, null, null));
    }

    private SmtpClient CreateClient() =>
        new(_credentials.Host, _credentials.Port)
        {
            Credentials = new NetworkCredential(_credentials.Username, _credentials.Password),
            EnableSsl = _credentials.UseSsl,
        };
}
