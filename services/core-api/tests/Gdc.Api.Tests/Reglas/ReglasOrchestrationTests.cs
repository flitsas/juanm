using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Notif.EmailSenders;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Provider;
using Gdc.Modules.Notif.Domain.Entities;
using Gdc.Modules.Reglas.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Execution;
using Gdc.Modules.Reglas.Application.Rules;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Gdc.Api.Tests.Reglas;

public sealed class ReglasOrchestrationTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PdfTemplateId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task GdcPdfTemplateRendererStub_returns_document_ref()
    {
        var renderer = new GdcPdfTemplateRendererStub();
        var result = await renderer.RenderAsync(
            PdfTemplateId,
            new Dictionary<string, string> { ["numero_comparendo"] = "CMP-1" },
            CancellationToken.None);

        Assert.NotEmpty(result.Content);
        Assert.Contains("CMP-1", result.DocumentRef);
    }

    [Fact]
    public async Task ProcessMatchesAsync_marks_record_success_with_pdf_and_email_refs()
    {
        await using var db = CreateDbContext();
        var secretariaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        await SeedSecretariatContactAsync(db, secretariaId.ToString());
        var ruleId = await SeedRuleWithContactAsync(db, secretariatContactId: null);
        var comparendoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        await SeedComparendoAsync(db, comparendoId, secretariaId);
        await SeedProviderAsync(db);

        var runId = Guid.CreateVersion7();
        db.RuleProcessingRecords.Add(new RuleProcessingRecord
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            DynamicRuleId = ruleId,
            ComparendoId = comparendoId,
            RuleExecutionRunId = runId,
            Status = ReglasProcessingStatuses.Matched,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.RuleExecutionRuns.Add(new RuleExecutionRun
        {
            Id = runId,
            TenantId = TenantId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = ReglasRunStatuses.Completed,
            TriggerType = ReglasTriggerTypes.Manual,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var sender = new RecordingEmailSender();
        var job = CreateOrchestrationJob(db, sender);
        var result = await job.ProcessMatchesAsync(TenantId, runId, CancellationToken.None);

        Assert.Equal(1, result.SucceededCount);
        var record = await db.RuleProcessingRecords.SingleAsync();
        Assert.Equal(ReglasProcessingStatuses.Success, record.Status);
        Assert.NotNull(record.PdfDocumentRef);
        Assert.NotNull(record.EmailSendRef);
        Assert.NotNull(sender.LastMessage);
        Assert.Single(sender.LastMessage!.Attachments!);
    }

    [Fact]
    public async Task ProcessMatchesAsync_fails_without_secretariat_contact()
    {
        await using var db = CreateDbContext();
        var ruleId = await SeedRuleWithContactAsync(db, secretariatContactId: null);
        var comparendoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        await SeedComparendoAsync(db, comparendoId);

        db.RuleProcessingRecords.Add(new RuleProcessingRecord
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            DynamicRuleId = ruleId,
            ComparendoId = comparendoId,
            Status = ReglasProcessingStatuses.Matched,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var job = CreateOrchestrationJob(db, new RecordingEmailSender());
        var result = await job.ProcessMatchesAsync(TenantId, null, CancellationToken.None);

        Assert.Equal(1, result.FailedCount);
        var record = await db.RuleProcessingRecords.SingleAsync();
        Assert.Equal(ReglasProcessingStatuses.Failed, record.Status);
        Assert.Contains("Secretariat", record.ErrorMessage);
    }

    private static ReglasOrchestrationJob CreateOrchestrationJob(GdcDbContext db, IEmailSender sender)
    {
        var factory = new FakeSenderFactory(sender);
        var encryptor = new FakeEncryptor();
        var dispatcher = new ReglasEmailDispatcher(db, factory, encryptor);
        return new ReglasOrchestrationJob(
            db,
            new DgcComparendoReader(db),
            new ReglasSecretariatResolver(db),
            new GdcPdfTemplateRendererStub(),
            dispatcher,
            new DgcEmailLogWriter(db, TimeProvider.System),
            TimeProvider.System,
            Options.Create(new ReglasExecutionOptions { BatchSize = 50 }),
            NullLogger<ReglasOrchestrationJob>.Instance);
    }

    private static async Task<Guid> SeedRuleWithContactAsync(GdcDbContext db, Guid? secretariatContactId)
    {
        var ruleId = Guid.CreateVersion7();
        db.DynamicRules.Add(new DynamicRule
        {
            Id = ruleId,
            TenantId = TenantId,
            Name = "DP Test",
            IsActive = true,
            PdfTemplateId = PdfTemplateId,
            EmailSubject = "DP {{numero_comparendo}}",
            EmailBodyHtml = "<p>DP {{infractor}}</p>",
            SecretariatContactId = secretariatContactId,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
        return ruleId;
    }

    private static async Task SeedSecretariatContactAsync(GdcDbContext db, string secretariatCode)
    {
        var contact = new SecretariatContact
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            SecretariatCode = secretariatCode,
            SecretariatName = "Bogotá",
            ContactName = "Operador",
            ContactEmail = "secretaria@example.com",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.SecretariatContacts.Add(contact);
        await db.SaveChangesAsync();
    }

    private static async Task SeedComparendoAsync(GdcDbContext db, Guid comparendoId, Guid? secretariaId = null)
    {
        db.Comparendos.Add(new Comparendo
        {
            Id = comparendoId,
            TenantId = TenantId,
            NumeroComparendo = "CMP-100",
            Estado = "Notificado",
            InfractorNombre = "Ana",
            Placa = "ABC123",
            FechaComparendo = new DateOnly(2026, 6, 1),
            SecretariaId = secretariaId,
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
            Correo = "ana@example.com",
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();
    }

    private static async Task SeedProviderAsync(GdcDbContext db)
    {
        db.EmailProviderConfigs.Add(new EmailProviderConfig
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            ProviderType = ProviderTypes.Api,
            FromAddress = "reglas@flit.dev",
            CredentialsEncrypted = "enc:test",
            IsActive = true,
            DispatchEnabled = true,
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

    private sealed class RecordingEmailSender : IEmailSender
    {
        public EmailMessage? LastMessage { get; private set; }

        public Task ValidateAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            LastMessage = message;
            return Task.FromResult(new EmailSendResult(true, "msg-reglas-1", null));
        }
    }

    private sealed class FakeSenderFactory(IEmailSender sender) : IEmailSenderFactory
    {
        public IEmailSender Create(string providerType, string credentialsJson) => sender;
    }

    private sealed class FakeEncryptor : IEmailCredentialEncryptor
    {
        public string Decrypt(string cipherText) => """{"baseUrl":"https://mail.test","apiKey":"k"}""";

        public string Encrypt(string plainText) => $"enc:{plainText}";
    }
}
