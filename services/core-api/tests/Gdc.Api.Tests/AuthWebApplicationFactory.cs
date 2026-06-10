using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Api.Tests;

public sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"gdc-api-auth-{Guid.NewGuid()}";
    private CapturingEmailSender? _emailSender;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("Testing:UseInMemoryDatabase", "true");
        builder.UseSetting("Testing:InMemoryDatabaseName", _databaseName);

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            _emailSender = new CapturingEmailSender();
            services.AddSingleton(_emailSender);
            services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<CapturingEmailSender>());
        });
    }

    public async Task SeedAsync(Func<GdcDbContext, Task> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GdcDbContext>();
        await seed(db);
    }

    public CapturingEmailSender GetEmailSender() =>
        _emailSender ?? Services.GetRequiredService<CapturingEmailSender>();

    public string GetActivationTokenFromLastEmail() => ExtractTokenFromLastEmail();

    public string GetPasswordResetTokenFromLastEmail() => ExtractTokenFromLastEmail();

    private string ExtractTokenFromLastEmail()
    {
        var body = GetEmailSender().LastMessage?.Body
            ?? throw new InvalidOperationException("No email captured.");
        const string marker = "token=";
        var start = body.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return body[start..];
    }
}
