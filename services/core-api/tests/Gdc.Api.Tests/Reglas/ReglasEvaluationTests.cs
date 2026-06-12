using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Evaluation;
using Gdc.Modules.Reglas.Application.Execution;
using Gdc.Modules.Reglas.Application.Rules;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Gdc.Api.Tests.Reglas;

public sealed class ReglasEvaluationTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PdfTemplateId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly DgcComparendoSnapshot Comparendo = new(
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        TenantId,
        "CMP-100",
        "Notificado",
        "Ana",
        "ABC123",
        new DateOnly(2026, 6, 1),
        null,
        "ana@example.com");

    [Fact]
    public void Matches_predicate_eq_on_estado()
    {
        var root = Predicate(ReglasFieldKeys.Estado, ReglasComparisonOperators.Equal, "Notificado");
        Assert.True(ReglasRuleEvaluator.Matches(root, Comparendo));
        Assert.False(ReglasRuleEvaluator.Matches(
            Predicate(ReglasFieldKeys.Estado, ReglasComparisonOperators.Equal, "Cerrado"),
            Comparendo));
    }

    [Fact]
    public void Matches_and_group_requires_all_children()
    {
        var root = new ConditionNodeDto(
            ReglasNodeTypes.Group,
            ReglasLogicOperators.And,
            null,
            null,
            null,
            [
                Predicate(ReglasFieldKeys.Estado, ReglasComparisonOperators.Equal, "Notificado"),
                Predicate(ReglasFieldKeys.Placa, ReglasComparisonOperators.Contains, "ABC"),
            ]);

        Assert.True(ReglasRuleEvaluator.Matches(root, Comparendo));

        var failing = new ConditionNodeDto(
            ReglasNodeTypes.Group,
            ReglasLogicOperators.And,
            null,
            null,
            null,
            [
                Predicate(ReglasFieldKeys.Estado, ReglasComparisonOperators.Equal, "Notificado"),
                Predicate(ReglasFieldKeys.Placa, ReglasComparisonOperators.Equal, "ZZZ"),
            ]);

        Assert.False(ReglasRuleEvaluator.Matches(failing, Comparendo));
    }

    [Fact]
    public void Matches_or_group_matches_any_child()
    {
        var root = new ConditionNodeDto(
            ReglasNodeTypes.Group,
            ReglasLogicOperators.Or,
            null,
            null,
            null,
            [
                Predicate(ReglasFieldKeys.Estado, ReglasComparisonOperators.Equal, "Cerrado"),
                Predicate(ReglasFieldKeys.Placa, ReglasComparisonOperators.Contains, "ABC"),
            ]);

        Assert.True(ReglasRuleEvaluator.Matches(root, Comparendo));
    }

    [Fact]
    public async Task RunForTenantAsync_records_matches_and_skips_success_pairs()
    {
        await using var db = CreateDbContext();
        var ruleId = await SeedActiveRuleAsync(db);
        var comparendoId = Comparendo.Id;
        await SeedComparendoAsync(db, comparendoId);

        db.RuleProcessingRecords.Add(new RuleProcessingRecord
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            DynamicRuleId = ruleId,
            ComparendoId = Guid.CreateVersion7(),
            Status = ReglasProcessingStatuses.Success,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var job = CreateExecutionJob(db);
        var (runId, matchedCount) = await job.RunForTenantAsync(
            TenantId,
            ReglasTriggerTypes.Manual,
            null,
            CancellationToken.None);

        Assert.Equal(1, matchedCount);
        var run = await db.RuleExecutionRuns.SingleAsync(r => r.Id == runId);
        Assert.Equal(ReglasRunStatuses.Completed, run.Status);
        Assert.Equal(1, run.MatchedCount);

        var match = await db.RuleProcessingRecords.SingleAsync(r => r.Status == ReglasProcessingStatuses.Matched);
        Assert.Equal(comparendoId, match.ComparendoId);
        Assert.Equal(ruleId, match.DynamicRuleId);
    }

    [Fact]
    public async Task RunForTenantAsync_skips_inactive_rules()
    {
        await using var db = CreateDbContext();
        await SeedComparendoAsync(db, Comparendo.Id);

        db.DynamicRules.Add(new DynamicRule
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            Name = "Inactiva",
            IsActive = false,
            PdfTemplateId = PdfTemplateId,
            EmailSubject = "Asunto",
            EmailBodyHtml = "<p>x</p>",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var job = CreateExecutionJob(db);
        var (_, matchedCount) = await job.RunForTenantAsync(
            TenantId,
            ReglasTriggerTypes.Manual,
            null,
            CancellationToken.None);

        Assert.Equal(0, matchedCount);
    }

    [Fact]
    public async Task ListMatchesAsync_returns_rule_and_comparendo_labels()
    {
        await using var db = CreateDbContext();
        var ruleId = await SeedActiveRuleAsync(db);
        await SeedComparendoAsync(db, Comparendo.Id);

        var job = CreateExecutionJob(db);
        await job.RunForTenantAsync(TenantId, ReglasTriggerTypes.Manual, null, CancellationToken.None);

        var orchestration = CreateOrchestrationJob(db);
        var service = new ReglasExecutionService(
            db,
            new FakeTenantContext(TenantId),
            ReglasTestSupport.TenantAdminAccess(),
            job,
            orchestration);
        var matches = await service.ListMatchesAsync(null, null, CancellationToken.None);

        Assert.Single(matches.Items);
        Assert.Equal("DP Test", matches.Items[0].RuleName);
        Assert.Equal("CMP-100", matches.Items[0].ComparendoNumero);
        Assert.Equal(ReglasProcessingStatuses.Matched, matches.Items[0].Status);
    }

    private static ConditionNodeDto Predicate(string field, string op, string value) =>
        new(
            ReglasNodeTypes.Predicate,
            null,
            field,
            op,
            value,
            null);

    private static async Task<Guid> SeedActiveRuleAsync(GdcDbContext db)
    {
        var ruleId = Guid.CreateVersion7();
        var groupId = Guid.CreateVersion7();
        var predicateId = Guid.CreateVersion7();
        var now = DateTimeOffset.UtcNow;

        db.DynamicRules.Add(new DynamicRule
        {
            Id = ruleId,
            TenantId = TenantId,
            Name = "DP Test",
            IsActive = true,
            PdfTemplateId = PdfTemplateId,
            EmailSubject = "Asunto",
            EmailBodyHtml = "<p>cuerpo</p>",
            CreatedAt = now,
        });

        db.RuleConditions.AddRange(
            new RuleCondition
            {
                Id = groupId,
                TenantId = TenantId,
                DynamicRuleId = ruleId,
                NodeType = ReglasNodeTypes.Group,
                LogicOperator = ReglasLogicOperators.And,
                SortOrder = 0,
                CreatedAt = now,
            },
            new RuleCondition
            {
                Id = predicateId,
                TenantId = TenantId,
                DynamicRuleId = ruleId,
                ParentId = groupId,
                NodeType = ReglasNodeTypes.Predicate,
                FieldKey = ReglasFieldKeys.Estado,
                ComparisonOperator = ReglasComparisonOperators.Equal,
                ComparisonValue = "Notificado",
                SortOrder = 0,
                CreatedAt = now,
            });

        await db.SaveChangesAsync();
        return ruleId;
    }

    private static async Task SeedComparendoAsync(GdcDbContext db, Guid comparendoId)
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

    private static ReglasExecutionJob CreateExecutionJob(GdcDbContext db) =>
        new(
            db,
            new DgcComparendoReader(db),
            CreateOrchestrationJob(db),
            TimeProvider.System,
            Options.Create(new ReglasExecutionOptions { BatchSize = 50, ProcessMatchesAfterEvaluation = false }),
            NullLogger<ReglasExecutionJob>.Instance);

    private static ReglasOrchestrationJob CreateOrchestrationJob(GdcDbContext db) =>
        new(
            db,
            new DgcComparendoReader(db),
            new ReglasSecretariatResolver(db),
            new GdcPdfTemplateRendererStub(),
            new ReglasEmailDispatcher(db, new NoopSenderFactory(), new NoopEncryptor()),
            new DgcEmailLogWriter(db, TimeProvider.System),
            TimeProvider.System,
            Options.Create(new ReglasExecutionOptions { BatchSize = 50 }),
            NullLogger<ReglasOrchestrationJob>.Instance);

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

        public Guid? UserId => Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    }
}
