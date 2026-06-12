using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Provider;
using Gdc.Modules.Reglas.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasEmailDispatcher(
    GdcDbContext db,
    IEmailSenderFactory senderFactory,
    IEmailCredentialEncryptor encryptor) : IReglasEmailDispatcher
{
    public async Task<ReglasEmailDispatchResult> DispatchAsync(
        ReglasEmailDispatchRequest request,
        CancellationToken cancellationToken)
    {
        var provider = await db.EmailProviderConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.TenantId == request.TenantId && p.IsActive && p.DeletedAt == null,
                cancellationToken);

        if (provider is null)
        {
            return new ReglasEmailDispatchResult(false, null, "No active NOTIF email provider configured.");
        }

        try
        {
            var credentialsJson = encryptor.Decrypt(provider.CredentialsEncrypted ?? string.Empty);
            var sender = senderFactory.Create(provider.ProviderType, credentialsJson);
            var attachment = new EmailAttachment(
                request.PdfFileName,
                "application/pdf",
                request.PdfContent);

            var result = await sender.SendAsync(
                new EmailMessage(
                    provider.FromAddress,
                    request.To,
                    request.Subject,
                    request.HtmlBody,
                    [attachment]),
                cancellationToken);

            return new ReglasEmailDispatchResult(
                result.Success,
                result.ProviderMessageId,
                result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return new ReglasEmailDispatchResult(false, null, ex.Message);
        }
    }
}
