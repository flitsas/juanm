using Gdc.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Gdc.Api.Endpoints;

public static class DevEndpoints
{
    public static void MapDevEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapGet("/api/v1/dev/notif-provider-prefill", (
            IOptions<DevNotifProviderPrefillOptions> options) =>
        {
            var prefill = options.Value;
            if (string.IsNullOrWhiteSpace(prefill.SmtpHost)
                || string.IsNullOrWhiteSpace(prefill.SmtpUsername)
                || string.IsNullOrWhiteSpace(prefill.SmtpPassword))
            {
                return Results.NotFound(new { message = "DevNotifProviderPrefill no configurado." });
            }

            return Results.Ok(new
            {
                providerType = prefill.ProviderType,
                fromAddress = prefill.FromAddress,
                smtpHost = prefill.SmtpHost,
                smtpPort = prefill.SmtpPort,
                smtpUsername = prefill.SmtpUsername,
                smtpPassword = prefill.SmtpPassword,
                smtpUseSsl = prefill.SmtpUseSsl,
                testDestino = prefill.TestDestino,
            });
        })
        .WithName("DevNotifProviderPrefill")
        .WithTags("Dev")
        .AllowAnonymous();
    }
}
