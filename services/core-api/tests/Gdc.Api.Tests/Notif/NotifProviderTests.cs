using System.Text.Json;
using Gdc.Infrastructure.Notif;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Provider;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Notif;

public sealed class NotifProviderTests
{
    private static readonly Guid TenantA = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TenantB = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task SaveProviderAsync_persists_encrypted_credentials()
    {
        await using var db = CreateDbContext();
        var encryptor = new FakeEncryptor();
        var service = CreateService(db, TenantA, encryptor, validationSucceeds: true);

        var credentials = JsonDocument.Parse("""{"apiKey":"secret-key"}""").RootElement;
        var saved = await service.SaveProviderAsync(
            new SaveProviderRequest(ProviderTypes.SendGrid, "notif@flit.dev", credentials),
            CancellationToken.None);

        Assert.True(saved.HasCredentials);
        Assert.Equal(ProviderTypes.SendGrid, saved.ProviderType);
        var stored = await db.EmailProviderConfigs.SingleAsync();
        Assert.StartsWith("enc:", stored.CredentialsEncrypted);
        Assert.NotEqual("secret-key", stored.CredentialsEncrypted);
    }

    [Fact]
    public async Task GetActiveProviderAsync_returns_only_current_tenant()
    {
        await using var db = CreateDbContext();
        SeedProviders(db);
        var serviceA = CreateService(db, TenantA, new FakeEncryptor(), validationSucceeds: true);
        var serviceB = CreateService(db, TenantB, new FakeEncryptor(), validationSucceeds: true);

        var providerA = await serviceA.GetActiveProviderAsync(CancellationToken.None);
        var providerB = await serviceB.GetActiveProviderAsync(CancellationToken.None);

        Assert.NotNull(providerA);
        Assert.NotNull(providerB);
        Assert.Equal(ProviderTypes.SendGrid, providerA.ProviderType);
        Assert.Equal(ProviderTypes.Api, providerB.ProviderType);
    }

    [Fact]
    public async Task SaveProviderAsync_does_not_activate_when_validation_fails()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db, TenantA, new FakeEncryptor(), validationSucceeds: false);
        var credentials = JsonDocument.Parse("""{"apiKey":"bad"}""").RootElement;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SaveProviderAsync(
                new SaveProviderRequest(ProviderTypes.SendGrid, "notif@flit.dev", credentials),
                CancellationToken.None));

        Assert.Empty(await db.EmailProviderConfigs.Where(p => p.IsActive).ToListAsync());
    }

    [Fact]
    public async Task TestProviderAsync_uses_active_configuration()
    {
        await using var db = CreateDbContext();
        SeedProviders(db);
        var sender = new RecordingEmailSender();
        var service = CreateService(db, TenantA, new FakeEncryptor(), validationSucceeds: true, sender);

        var result = await service.TestProviderAsync(
            new TestProviderRequest("ops@tenant.test", null, null, null),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("ops@tenant.test", sender.LastMessage?.To);
    }

    private static void SeedProviders(GdcDbContext db)
    {
        db.EmailProviderConfigs.AddRange(
            new EmailProviderConfig
            {
                Id = Guid.CreateVersion7(),
                TenantId = TenantA,
                ProviderType = ProviderTypes.SendGrid,
                FromAddress = "a@flit.dev",
                CredentialsEncrypted = "enc:tenant-a",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new EmailProviderConfig
            {
                Id = Guid.CreateVersion7(),
                TenantId = TenantB,
                ProviderType = ProviderTypes.Api,
                FromAddress = "b@flit.dev",
                CredentialsEncrypted = "enc:tenant-b",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        db.SaveChanges();
    }

    private static NotifProviderService CreateService(
        GdcDbContext db,
        Guid tenantId,
        IEmailCredentialEncryptor encryptor,
        bool validationSucceeds,
        RecordingEmailSender? sender = null)
    {
        sender ??= new RecordingEmailSender { ValidationSucceeds = validationSucceeds };
        var factory = new FakeEmailSenderFactory(sender);
        return new NotifProviderService(db, new FakeTenantContext(tenantId), factory, encryptor, TimeProvider.System);
    }

    private static GdcDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GdcDbContext(options);
    }

    private sealed class FakeTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }

    private sealed class FakeEncryptor : IEmailCredentialEncryptor
    {
        public string Encrypt(string plainText) => "enc:" + plainText;

        public string Decrypt(string cipherText) => cipherText["enc:".Length..];
    }

    private sealed class RecordingEmailSender : IEmailSender
    {
        public bool ValidationSucceeds { get; set; } = true;

        public EmailMessage? LastMessage { get; private set; }

        public Task ValidateAsync(CancellationToken cancellationToken)
        {
            if (!ValidationSucceeds)
            {
                throw new InvalidOperationException("Invalid provider credentials.");
            }

            return Task.CompletedTask;
        }

        public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            LastMessage = message;
            return Task.FromResult(new EmailSendResult(true, "msg-1", null));
        }
    }

    private sealed class FakeEmailSenderFactory(RecordingEmailSender sender) : IEmailSenderFactory
    {
        public IEmailSender Create(string providerType, string credentialsJson) => sender;
    }
}
