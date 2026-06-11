using Gdc.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Tests;

public class SmtpEmailSenderTests
{
    [Fact]
    public void AuthDependencyInjection_registers_smtp_sender_when_smtp_enabled_env_var()
    {
        Environment.SetEnvironmentVariable(SmtpSettings.EnvEnabled, "true");
        Environment.SetEnvironmentVariable(SmtpSettings.EnvHost, "smtp.office365.com");

        try
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthServices(configuration);

            var sender = services.BuildServiceProvider().GetRequiredService<IEmailSender>();
            Assert.IsType<SmtpEmailSender>(sender);
        }
        finally
        {
            Environment.SetEnvironmentVariable(SmtpSettings.EnvEnabled, null);
            Environment.SetEnvironmentVariable(SmtpSettings.EnvHost, null);
        }
    }

    [Fact]
    public void AuthDependencyInjection_registers_smtp_sender_when_enabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Enabled"] = "true",
                ["Smtp:Host"] = "smtp.office365.com",
                ["Smtp:Port"] = "587",
                ["Smtp:EnableSsl"] = "true",
                ["Smtp:FromAddress"] = "noreply@example.com",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthServices(configuration);

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IEmailSender>();

        Assert.IsType<SmtpEmailSender>(sender);
    }

    [Fact]
    public void AuthDependencyInjection_registers_logging_sender_when_disabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Enabled"] = "false",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthServices(configuration);

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IEmailSender>();

        Assert.IsType<LoggingEmailSender>(sender);
    }

    [Fact]
    public void Bind_reads_smtp_environment_variables()
    {
        Environment.SetEnvironmentVariable(SmtpSettings.EnvHost, "smtp.office365.com");
        Environment.SetEnvironmentVariable(SmtpSettings.EnvPort, "587");
        Environment.SetEnvironmentVariable(SmtpSettings.EnvUser, "user@example.com");
        Environment.SetEnvironmentVariable(SmtpSettings.EnvPassword, "secret");
        Environment.SetEnvironmentVariable(SmtpSettings.EnvFrom, "user@example.com");
        Environment.SetEnvironmentVariable(SmtpSettings.EnvFromName, "FLIT Tramites");
        Environment.SetEnvironmentVariable(SmtpSettings.EnvEnabled, "true");

        try
        {
            var configuration = new ConfigurationBuilder().Build();
            var settings = new SmtpSettings();
            SmtpSettings.Bind(configuration, settings);

            Assert.Equal("smtp.office365.com", settings.Host);
            Assert.Equal(587, settings.Port);
            Assert.Equal("user@example.com", settings.Username);
            Assert.Equal("secret", settings.Password);
            Assert.Equal("user@example.com", settings.FromAddress);
            Assert.Equal("FLIT Tramites", settings.FromDisplayName);
            Assert.True(settings.Enabled);
        }
        finally
        {
            Environment.SetEnvironmentVariable(SmtpSettings.EnvHost, null);
            Environment.SetEnvironmentVariable(SmtpSettings.EnvPort, null);
            Environment.SetEnvironmentVariable(SmtpSettings.EnvUser, null);
            Environment.SetEnvironmentVariable(SmtpSettings.EnvPassword, null);
            Environment.SetEnvironmentVariable(SmtpSettings.EnvFrom, null);
            Environment.SetEnvironmentVariable(SmtpSettings.EnvFromName, null);
            Environment.SetEnvironmentVariable(SmtpSettings.EnvEnabled, null);
        }
    }

    [Fact]
    public async Task SendAsync_throws_when_from_address_missing()
    {
        var sender = new SmtpEmailSender(
            Options.Create(new SmtpSettings
            {
                Host = "smtp.office365.com",
                Port = 587,
                EnableSsl = true,
            }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SmtpEmailSender>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sender.SendAsync(new EmailMessage("user@example.com", "Test", "Body")));
    }

    [Fact]
    public async Task SendAsync_throws_when_recipient_invalid()
    {
        var sender = new SmtpEmailSender(
            Options.Create(new SmtpSettings
            {
                Host = "smtp.office365.com",
                Port = 587,
                EnableSsl = true,
                FromAddress = "noreply@example.com",
            }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SmtpEmailSender>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sender.SendAsync(new EmailMessage("not-an-email", "Test", "Body")));
    }
}
