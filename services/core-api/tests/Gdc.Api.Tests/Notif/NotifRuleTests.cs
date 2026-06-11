using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Notif;
using Gdc.Infrastructure.Notif.EmailSenders;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Provider;
using Gdc.Modules.Notif.Application.Rules;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gdc.Api.Tests.Notif;

public sealed class NotifRuleTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task CreateAsync_persists_chronological_rule()
    {
        await using var db = CreateDbContext();
        var templateId = await SeedTemplateAsync(db);
        var service = CreateRuleService(db);

        var created = await service.CreateAsync(
            new CreateRuleRequest(
                templateId,
                "Recordatorio 3 días",
                RuleTriggerTypes.Chronological,
                3,
                TriggerReferences.FechaNotificacion,
                null,
                true),
            CancellationToken.None);

        Assert.Equal(templateId, created.EmailTemplateId);
        Assert.Equal(RuleTriggerTypes.Chronological, created.TriggerType);
        Assert.Single(await db.NotificationRules.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_rejects_active_rule_without_template()
    {
        await using var db = CreateDbContext();
        var service = CreateRuleService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                new CreateRuleRequest(
                    Guid.Empty,
                    "Sin plantilla",
                    RuleTriggerTypes.State,
                    null,
                    null,
                    "Notificado",
                    true),
                CancellationToken.None));
    }

    [Fact]
    public void Chronological_rule_is_eligible_after_target_date()
    {
        var rule = new NotificationRule
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            EmailTemplateId = Guid.CreateVersion7(),
            Name = "R1",
            TriggerType = RuleTriggerTypes.Chronological,
            TriggerDays = 3,
            TriggerReference = TriggerReferences.FechaComparendo,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var comparendo = new DgcComparendoSnapshot(
            Guid.CreateVersion7(),
            TenantId,
            "CMP-001",
            "Notificado",
            "Juan",
            "ABC123",
            new DateOnly(2026, 6, 1),
            null,
            "juan@example.com");

        Assert.False(NotifRuleEvaluator.IsEligible(rule, comparendo, new DateOnly(2026, 6, 3)));
        Assert.True(NotifRuleEvaluator.IsEligible(rule, comparendo, new DateOnly(2026, 6, 4)));
    }

    [Fact]
    public async Task RunCycleAsync_with_switch_off_skips_enqueue_but_dispatches_pending()
    {
        await using var db = CreateDbContext();
        var templateId = await SeedTemplateAsync(db);
        var comparendoId = await SeedComparendoAsync(db, "cmp@tenant.test");
        await SeedProviderAsync(db, dispatchEnabled: false);

        var rule = new NotificationRule
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            EmailTemplateId = templateId,
            Name = "Estado notificado",
            TriggerType = RuleTriggerTypes.State,
            TriggerEstado = "Notificado",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.NotificationRules.Add(rule);

        db.EmailQueues.Add(new EmailQueue
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            NotificationRuleId = rule.Id,
            EmailTemplateId = templateId,
            ComparendoId = comparendoId,
            Destino = "cmp@tenant.test",
            Status = QueueStatuses.Pending,
            ScheduledAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var job = CreateDispatchJob(db, new FakeEmailSender(success: true));
        await job.RunCycleAsync(CancellationToken.None);

        Assert.Empty(await db.EmailQueues.Where(q => q.NotificationRuleId == rule.Id && q.Id != db.EmailQueues.First().Id).ToListAsync());
        var processed = await db.EmailQueues.SingleAsync();
        Assert.Equal(QueueStatuses.Sent, processed.Status);
        Assert.Single(await db.EmailLogs.ToListAsync());
    }

    [Fact]
    public async Task Dispatch_writes_dgc_email_log_with_html_evidence()
    {
        await using var db = CreateDbContext();
        var templateId = await SeedTemplateAsync(db);
        var comparendoId = await SeedComparendoAsync(db, "ops@tenant.test");
        await SeedProviderAsync(db, dispatchEnabled: true);

        var ruleId = Guid.CreateVersion7();
        db.NotificationRules.Add(new NotificationRule
        {
            Id = ruleId,
            TenantId = TenantId,
            EmailTemplateId = templateId,
            Name = "Notificación inicial",
            TriggerType = RuleTriggerTypes.State,
            TriggerEstado = "Notificado",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        db.EmailQueues.Add(new EmailQueue
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            NotificationRuleId = ruleId,
            EmailTemplateId = templateId,
            ComparendoId = comparendoId,
            Destino = "ops@tenant.test",
            Status = QueueStatuses.Pending,
            ScheduledAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var job = CreateDispatchJob(db, new FakeEmailSender(success: true));
        await job.RunCycleAsync(CancellationToken.None);

        var log = await db.EmailLogs.SingleAsync();
        Assert.Equal(comparendoId, log.ComparendoId);
        Assert.Equal("ops@tenant.test", log.Destino);
        Assert.Equal("Entregado", log.EstadoEntrega);
        Assert.Contains("<p>", log.HtmlEvidencia);
    }

    [Fact]
    public async Task UpdateSwitchAsync_persists_dispatch_enabled_flag()
    {
        await using var db = CreateDbContext();
        await SeedProviderAsync(db, dispatchEnabled: true);
        var service = new NotifSwitchService(db, new FakeTenantContext(TenantId), TimeProvider.System);

        var updated = await service.UpdateAsync(new UpdateSwitchRequest(false), CancellationToken.None);

        Assert.False(updated.DispatchEnabled);
        Assert.False((await db.EmailProviderConfigs.SingleAsync()).DispatchEnabled);
    }

    private static NotifRuleService CreateRuleService(GdcDbContext db) =>
        new(db, new FakeTenantContext(TenantId), TimeProvider.System);

    private static NotifDispatchJob CreateDispatchJob(GdcDbContext db, IEmailSender sender)
    {
        var factory = new FakeSenderFactory(sender);
        var encryptor = new FakeEncryptor();
        var reader = new DgcComparendoReader(db);
        var writer = new DgcEmailLogWriter(db, TimeProvider.System);
        return new NotifDispatchJob(
            db,
            reader,
            factory,
            encryptor,
            writer,
            TimeProvider.System,
            NullLogger<NotifDispatchJob>.Instance);
    }

    private static async Task<Guid> SeedTemplateAsync(GdcDbContext db)
    {
        var template = new EmailTemplate
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            Name = "Plantilla base",
            Subject = "Comparendo {{numero_comparendo}}",
            HtmlBody = "<p>Hola {{infractor}}</p>",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync();
        return template.Id;
    }

    private static async Task<Guid> SeedComparendoAsync(GdcDbContext db, string email)
    {
        var comparendoId = Guid.CreateVersion7();
        db.Comparendos.Add(new Comparendo
        {
            Id = comparendoId,
            TenantId = TenantId,
            NumeroComparendo = "CMP-100",
            Estado = "Notificado",
            InfractorNombre = "Ana",
            Placa = "XYZ99",
            FechaComparendo = new DateOnly(2026, 6, 1),
            Fuente = "OCR",
            PendienteContraventor = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        db.Contraventors.Add(new Contraventor
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            ComparendoId = comparendoId,
            Nombre = "Ana",
            Documento = "123",
            Correo = email,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();
        return comparendoId;
    }

    private static async Task SeedProviderAsync(GdcDbContext db, bool dispatchEnabled)
    {
        db.EmailProviderConfigs.Add(new EmailProviderConfig
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            ProviderType = ProviderTypes.Api,
            FromAddress = "notif@flit.dev",
            CredentialsEncrypted = "enc:test",
            IsActive = true,
            DispatchEnabled = dispatchEnabled,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
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
        public string Decrypt(string cipherText) => """{"endpoint":"https://mail.test/send","apiKey":"k"}""";

        public string Encrypt(string plainText) => $"enc:{plainText}";
    }

    private sealed class FakeSenderFactory(IEmailSender sender) : IEmailSenderFactory
    {
        public IEmailSender Create(string providerType, string credentialsJson) => sender;
    }

    private sealed class FakeEmailSender(bool success) : IEmailSender
    {
        public Task ValidateAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken) =>
            Task.FromResult(new EmailSendResult(success, "msg-1", success ? null : "smtp error"));
    }
}
