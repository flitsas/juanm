using FluentValidation;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Reglas.Application.Rules;
using Gdc.Modules.Reglas.Application.Rules.Validators;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Reglas;

public sealed class ReglasRuleTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PdfTemplateId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly ConditionNodeDto SampleRoot = new(
        ReglasNodeTypes.Group,
        ReglasLogicOperators.And,
        null,
        null,
        null,
        [
            new ConditionNodeDto(
                ReglasNodeTypes.Predicate,
                null,
                ReglasFieldKeys.Estado,
                ReglasComparisonOperators.Equal,
                "Notificado",
                null),
        ]);

    [Fact]
    public async Task CreateAsync_persists_rule_with_condition_tree()
    {
        await using var db = CreateDbContext();
        var service = CreateRuleService(db);

        var created = await service.CreateAsync(
            new CreateReglasRuleRequest(
                "DP Secretaría Bogotá",
                "Regla piloto",
                true,
                PdfTemplateId,
                "DP {{numero_comparendo}}",
                "<p>Adjunto DP</p>",
                null,
                SampleRoot),
            CancellationToken.None);

        Assert.Equal("DP Secretaría Bogotá", created.Name);
        Assert.NotNull(created.ConditionRoot);
        Assert.Equal(ReglasNodeTypes.Predicate, created.ConditionRoot!.Children![0].NodeType);
        Assert.Single(await db.DynamicRules.ToListAsync());
        Assert.Equal(2, await db.RuleConditions.CountAsync());
    }

    [Fact]
    public async Task CreateReglasRuleRequestValidator_rejects_active_rule_without_pdf_template()
    {
        var validator = new CreateReglasRuleRequestValidator();
        var result = await validator.ValidateAsync(
            new CreateReglasRuleRequest(
                "Sin plantilla",
                null,
                true,
                Guid.Empty,
                "Asunto",
                "<p>cuerpo</p>",
                null,
                SampleRoot));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReglasRuleRequest.PdfTemplateId));
    }

    [Fact]
    public async Task CreateReglasRuleRequestValidator_rejects_active_rule_without_conditions()
    {
        var validator = new CreateReglasRuleRequestValidator();
        var result = await validator.ValidateAsync(
            new CreateReglasRuleRequest(
                "Sin condiciones",
                null,
                true,
                PdfTemplateId,
                "Asunto",
                "<p>cuerpo</p>",
                null,
                null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReglasRuleRequest.ConditionRoot));
    }

    [Fact]
    public async Task UpdateAsync_replaces_condition_tree()
    {
        await using var db = CreateDbContext();
        var service = CreateRuleService(db);
        var created = await service.CreateAsync(
            new CreateReglasRuleRequest(
                "Original",
                null,
                false,
                PdfTemplateId,
                "Asunto",
                "<p>cuerpo</p>",
                null,
                SampleRoot),
            CancellationToken.None);

        var updatedRoot = new ConditionNodeDto(
            ReglasNodeTypes.Group,
            ReglasLogicOperators.Or,
            null,
            null,
            null,
            [
                new ConditionNodeDto(
                    ReglasNodeTypes.Predicate,
                    null,
                    ReglasFieldKeys.Placa,
                    ReglasComparisonOperators.Contains,
                    "ABC",
                    null),
            ]);

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateReglasRuleRequest(
                "Actualizada",
                "desc",
                true,
                PdfTemplateId,
                "Nuevo asunto",
                "<p>nuevo</p>",
                null,
                updatedRoot),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Actualizada", updated!.Name);
        Assert.True(updated.IsActive);
        Assert.Equal(ReglasLogicOperators.Or, updated.ConditionRoot!.LogicOperator);
        Assert.Equal(2, await db.RuleConditions.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_soft_deletes_rule()
    {
        await using var db = CreateDbContext();
        var service = CreateRuleService(db);
        var created = await service.CreateAsync(
            new CreateReglasRuleRequest(
                "Borrar",
                null,
                false,
                PdfTemplateId,
                "Asunto",
                "<p>cuerpo</p>",
                null,
                null),
            CancellationToken.None);

        Assert.True(await service.DeleteAsync(created.Id, CancellationToken.None));
        Assert.Empty(await db.DynamicRules.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_validates_secretariat_contact_exists()
    {
        await using var db = CreateDbContext();
        var service = CreateRuleService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                new CreateReglasRuleRequest(
                    "Con contacto inválido",
                    null,
                    false,
                    PdfTemplateId,
                    "Asunto",
                    "<p>cuerpo</p>",
                    Guid.CreateVersion7(),
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_returns_nested_conditions()
    {
        await using var db = CreateDbContext();
        var contactId = await SeedSecretariatContactAsync(db);
        var service = CreateRuleService(db);
        var created = await service.CreateAsync(
            new CreateReglasRuleRequest(
                "Con contacto",
                null,
                true,
                PdfTemplateId,
                "Asunto",
                "<p>cuerpo</p>",
                contactId,
                SampleRoot),
            CancellationToken.None);

        var loaded = await service.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(contactId, loaded!.SecretariatContactId);
        Assert.Equal(ReglasFieldKeys.Estado, loaded.ConditionRoot!.Children![0].FieldKey);
    }

    private static ReglasRuleService CreateRuleService(GdcDbContext db) =>
        new(db, new FakeTenantContext(TenantId), TimeProvider.System);

    private static async Task<Guid> SeedSecretariatContactAsync(GdcDbContext db)
    {
        var contact = new SecretariatContact
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            SecretariatCode = "BOG",
            SecretariatName = "Bogotá",
            ContactName = "Operador",
            ContactEmail = "secretaria@example.com",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.SecretariatContacts.Add(contact);
        await db.SaveChangesAsync();
        return contact.Id;
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
}
