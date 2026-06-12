using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Reglas.Application.Secretariat;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Reglas;

public sealed class ReglasSecretariatContactTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SecretariaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task CreateAsync_persists_secretariat_contact()
    {
        await using var db = CreateDbContext();
        var service = CreateContactService(db, ReglasTestSupport.TenantAdminAccess());

        var created = await service.CreateAsync(
            new CreateReglasSecretariatContactRequest(
                SecretariaId.ToString(),
                "Bogotá",
                "Operador",
                "secretaria@example.com",
                "3001234567"),
            CancellationToken.None);

        Assert.Equal(SecretariaId.ToString(), created.SecretariatCode);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task CreateAsync_rejects_duplicate_secretariat_code()
    {
        await using var db = CreateDbContext();
        var service = CreateContactService(db, ReglasTestSupport.TenantAdminAccess());

        await service.CreateAsync(
            new CreateReglasSecretariatContactRequest("BOG", "Bogotá", "A", "a@example.com", null),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(
                new CreateReglasSecretariatContactRequest("BOG", "Bogotá", "B", "b@example.com", null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Operator_cannot_create_contact()
    {
        await using var db = CreateDbContext();
        var service = CreateContactService(db, ReglasTestSupport.OperatorAccess());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.CreateAsync(
                new CreateReglasSecretariatContactRequest("BOG", "Bogotá", "A", "a@example.com", null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Resolver_prefers_comparendo_secretaria_over_rule_contact()
    {
        await using var db = CreateDbContext();
        var comparendoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var ruleContactId = await SeedContactAsync(db, "RULE-CONTACT", "rule@example.com");
        await SeedContactAsync(db, SecretariaId.ToString(), "secretaria@example.com");
        await SeedComparendoAsync(db, comparendoId, SecretariaId);

        var resolver = new ReglasSecretariatResolver(db);
        var resolved = await resolver.ResolveAsync(TenantId, comparendoId, ruleContactId, CancellationToken.None);

        Assert.NotNull(resolved);
        Assert.Equal("secretaria@example.com", resolved!.ContactEmail);
    }

    [Fact]
    public async Task Operator_can_list_contacts()
    {
        await using var db = CreateDbContext();
        await SeedContactAsync(db, "BOG", "secretaria@example.com");
        var service = CreateContactService(db, ReglasTestSupport.OperatorAccess());

        var list = await service.ListAsync(CancellationToken.None);

        Assert.Single(list.Items);
    }

    [Fact]
    public async Task Operator_cannot_trigger_manual_run()
    {
        await using var db = CreateDbContext();
        var orchestration = CreateOrchestrationJob(db);
        var executionJob = new ReglasExecutionJob(
            db,
            new Gdc.Infrastructure.Dgc.DgcComparendoReader(db),
            orchestration,
            TimeProvider.System,
            Microsoft.Extensions.Options.Options.Create(new ReglasExecutionOptions { ProcessMatchesAfterEvaluation = false }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ReglasExecutionJob>.Instance);

        var operatorService = new ReglasExecutionService(
            db,
            new FakeTenantContext(TenantId),
            ReglasTestSupport.OperatorAccess(),
            executionJob,
            orchestration);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            operatorService.TriggerManualRunAsync(CancellationToken.None));
    }

    [Fact]
    public async Task TenantAdmin_can_trigger_manual_run()
    {
        await using var db = CreateDbContext();
        var orchestration = CreateOrchestrationJob(db);
        var executionJob = new ReglasExecutionJob(
            db,
            new Gdc.Infrastructure.Dgc.DgcComparendoReader(db),
            orchestration,
            TimeProvider.System,
            Microsoft.Extensions.Options.Options.Create(new ReglasExecutionOptions { ProcessMatchesAfterEvaluation = false }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ReglasExecutionJob>.Instance);

        var adminService = new ReglasExecutionService(
            db,
            new FakeTenantContext(TenantId),
            ReglasTestSupport.TenantAdminAccess(),
            executionJob,
            orchestration);

        var result = await adminService.TriggerManualRunAsync(CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.RunId);
    }

    private static ReglasOrchestrationJob CreateOrchestrationJob(GdcDbContext db) =>
        new(
            db,
            new Gdc.Infrastructure.Dgc.DgcComparendoReader(db),
            new ReglasSecretariatResolver(db),
            new GdcPdfTemplateRendererStub(),
            new ReglasEmailDispatcher(db, new NoopSenderFactory(), new NoopEncryptor()),
            new Gdc.Infrastructure.Dgc.DgcEmailLogWriter(db, TimeProvider.System),
            TimeProvider.System,
            Microsoft.Extensions.Options.Options.Create(new ReglasExecutionOptions()),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ReglasOrchestrationJob>.Instance);

    private static ReglasSecretariatContactService CreateContactService(
        GdcDbContext db,
        ReglasAccessService access) =>
        new(db, new FakeTenantContext(TenantId), access, TimeProvider.System);

    private static async Task<Guid> SeedContactAsync(GdcDbContext db, string code, string email)
    {
        var contact = new SecretariatContact
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            SecretariatCode = code,
            SecretariatName = "Secretaría",
            ContactName = "Operador",
            ContactEmail = email,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.SecretariatContacts.Add(contact);
        await db.SaveChangesAsync();
        return contact.Id;
    }

    private static async Task SeedComparendoAsync(GdcDbContext db, Guid comparendoId, Guid secretariaId)
    {
        db.Comparendos.Add(new Comparendo
        {
            Id = comparendoId,
            TenantId = TenantId,
            NumeroComparendo = "CMP-100",
            Estado = "Notificado",
            SecretariaId = secretariaId,
            Fuente = "OCR",
            PendienteContraventor = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private static GdcDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GdcDbContext(options, new TestSupport.TestTenantContext());
    }

    private sealed class FakeTenantContext(Guid tenantId) : Gdc.Modules.Dgc.Application.Abstractions.ITenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }

    private sealed class NoopSenderFactory : Gdc.Modules.Notif.Application.Abstractions.IEmailSenderFactory
    {
        public Gdc.Modules.Notif.Application.Abstractions.IEmailSender Create(string providerType, string credentialsJson) =>
            new NoopEmailSender();
    }

    private sealed class NoopEmailSender : Gdc.Modules.Notif.Application.Abstractions.IEmailSender
    {
        public Task ValidateAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Gdc.Modules.Notif.Application.Abstractions.EmailSendResult> SendAsync(
            Gdc.Modules.Notif.Application.Abstractions.EmailMessage message,
            CancellationToken cancellationToken) =>
            Task.FromResult(new Gdc.Modules.Notif.Application.Abstractions.EmailSendResult(true, "noop", null));
    }

    private sealed class NoopEncryptor : Gdc.Modules.Notif.Application.Abstractions.IEmailCredentialEncryptor
    {
        public string Decrypt(string cipherText) => "{}";

        public string Encrypt(string plainText) => plainText;
    }
}
