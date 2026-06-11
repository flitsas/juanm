namespace Gdc.Modules.Notif.Application.Abstractions;

public sealed record EmailMessage(string From, string To, string Subject, string HtmlBody);

public sealed record EmailSendResult(bool Success, string? ProviderMessageId, string? ErrorMessage);

public interface IEmailSender
{
    Task ValidateAsync(CancellationToken cancellationToken);

    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public interface IEmailSenderFactory
{
    IEmailSender Create(string providerType, string credentialsJson);
}

public interface IEmailCredentialEncryptor
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
