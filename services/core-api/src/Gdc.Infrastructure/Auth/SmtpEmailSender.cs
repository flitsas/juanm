using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Auth;

public sealed class SmtpEmailSender(
    IOptions<SmtpSettings> smtpOptions,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private const int DefaultPort = 587;
    private const int SendTimeoutMs = 300_000;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var settings = smtpOptions.Value;
        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            throw new InvalidOperationException($"{SmtpSettings.EnvHost} no está configurado.");
        }

        if (string.IsNullOrWhiteSpace(settings.FromAddress))
        {
            throw new InvalidOperationException($"{SmtpSettings.EnvFrom} no está configurado.");
        }

        if (!EmailAddressValidator.IsValid(message.To))
        {
            throw new ArgumentException("Dirección de correo destino no válida.", nameof(message));
        }

        using var mailMessage = BuildMailMessage(settings, message);
        using var smtpClient = CreateSmtpClient(settings);

        try
        {
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            logger.LogInformation(
                "Correo enviado vía SMTP a {Recipient}. Asunto: {Subject}",
                message.To,
                message.Subject);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al enviar correo vía SMTP ({Host}:{Port}). Asunto: {Subject}",
                settings.Host,
                settings.Port == 0 ? DefaultPort : settings.Port,
                message.Subject);
            throw;
        }
    }

    private static MailMessage BuildMailMessage(SmtpSettings settings, EmailMessage message)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromDisplayName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = false,
            Priority = MailPriority.Normal,
            DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
        };

        mailMessage.To.Add(message.To);
        return mailMessage;
    }

    private static SmtpClient CreateSmtpClient(SmtpSettings settings)
    {
        var port = settings.Port == 0 ? DefaultPort : settings.Port;
        var credentialUser = !string.IsNullOrWhiteSpace(settings.Username)
            ? settings.Username
            : settings.FromAddress;

#pragma warning disable SYSLIB0014 // ServicePointManager requerido para compatibilidad SMTP legacy (certificados autofirmados).
        ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
#pragma warning restore SYSLIB0014

        return new SmtpClient
        {
            Host = settings.Host,
            Port = port,
            EnableSsl = settings.EnableSsl,
            UseDefaultCredentials = false,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(credentialUser, settings.Password),
            Timeout = SendTimeoutMs,
        };
    }

    private static bool AcceptAllCertifications(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors) => true;
}
