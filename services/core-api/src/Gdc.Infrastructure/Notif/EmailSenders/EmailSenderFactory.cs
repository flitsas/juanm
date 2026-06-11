using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Provider;

namespace Gdc.Infrastructure.Notif.EmailSenders;

public sealed class EmailSenderFactory(IHttpClientFactory httpClientFactory) : IEmailSenderFactory
{
    public IEmailSender Create(string providerType, string credentialsJson) =>
        providerType switch
        {
            ProviderTypes.Api => new ApiEmailSender(httpClientFactory.CreateClient("NotifEmailApi"), credentialsJson),
            ProviderTypes.SendGrid => new SendGridEmailSender(httpClientFactory.CreateClient("NotifSendGrid"), credentialsJson),
            ProviderTypes.FlitMail => new FlitMailEmailSender(credentialsJson),
            _ => throw new ArgumentException($"Unsupported provider type: {providerType}", nameof(providerType)),
        };
}
