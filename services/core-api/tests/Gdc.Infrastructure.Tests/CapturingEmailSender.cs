using Gdc.Infrastructure.Auth;

namespace Gdc.Infrastructure.Tests;

public sealed class CapturingEmailSender : IEmailSender
{
    public EmailMessage? LastMessage { get; private set; }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        LastMessage = message;
        return Task.CompletedTask;
    }
}
