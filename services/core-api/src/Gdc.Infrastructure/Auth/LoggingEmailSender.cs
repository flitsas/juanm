using Microsoft.Extensions.Logging;

namespace Gdc.Infrastructure.Auth;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email enviado a destinatario (PII redactado). Asunto: {Subject}", message.Subject);
        return Task.CompletedTask;
    }
}
