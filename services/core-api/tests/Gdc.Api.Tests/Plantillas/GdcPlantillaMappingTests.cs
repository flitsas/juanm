using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Plantillas;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Plantillas.Application;
using Gdc.Modules.Plantillas.Application.Templates;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Plantillas;

public sealed class GdcPlantillaMappingTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void SystemVariableCatalog_contains_comparendo_and_contraventor_keys()
    {
        Assert.True(SystemVariableCatalog.IsKnown("comparendo.numero"));
        Assert.True(SystemVariableCatalog.IsKnown("contraventor.correo"));
        Assert.False(SystemVariableCatalog.IsKnown("invalid.key"));
    }

    [Fact]
    public async Task UploadAsync_creates_inactive_template_until_mapped()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        await using var stream = OpenFixturePdf();

        var response = await service.UploadAsync(
            stream,
            "vialix-dp-template.pdf",
            "application/pdf",
            "DP Vialix",
            CancellationToken.None);

        var template = await db.PdfTemplates.SingleAsync(t => t.Id == response.Id);
        Assert.False(template.IsActive);
    }

    [Fact]
    public async Task ActivateAsync_rejects_template_with_unmapped_fields()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var templateId = await UploadFixtureTemplate(service);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ActivateAsync(templateId, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateFieldMappingsAsync_supports_text_number_and_choice_types()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var templateId = await UploadFixtureTemplate(service);
        var fields = await db.PdfTemplateFields
            .Where(f => f.PdfTemplateId == templateId)
            .OrderBy(f => f.SortOrder)
            .ToListAsync();

        var numero = fields.First(f => f.AcroformName == "comparendo_numero");
        var total = fields.First(f => f.AcroformName == "comparendo_total");
        var estado = fields.First(f => f.AcroformName == "comparendo_estado");

        var detail = await service.UpdateFieldMappingsAsync(
            templateId,
            new UpdateFieldMappingsRequest([
                new UpdateFieldMappingRequest(numero.Id, PlantillaFieldTypes.Text, "comparendo.numero", null),
                new UpdateFieldMappingRequest(total.Id, PlantillaFieldTypes.Number, "comparendo.total", null),
                new UpdateFieldMappingRequest(
                    estado.Id,
                    PlantillaFieldTypes.Choice,
                    "comparendo.estado",
                    ["Notificado", "En proceso", "Cerrado"]),
            ]),
            CancellationToken.None);

        Assert.NotNull(detail);
        var mappedNumero = detail!.Fields.Single(f => f.Id == numero.Id);
        Assert.Equal("comparendo.numero", mappedNumero.SystemVariable);
        Assert.Equal(PlantillaFieldTypes.Text, mappedNumero.FieldType);

        var mappedEstado = detail.Fields.Single(f => f.Id == estado.Id);
        Assert.Equal(PlantillaFieldTypes.Choice, mappedEstado.FieldType);
        Assert.Equal(["Notificado", "En proceso", "Cerrado"], mappedEstado.ChoiceOptions);
    }

    [Fact]
    public async Task ActivateAsync_succeeds_when_all_fields_mapped()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var templateId = await UploadFixtureTemplate(service);
        var fields = await db.PdfTemplateFields
            .Where(f => f.PdfTemplateId == templateId)
            .ToListAsync();

        var mappings = fields.Select(MapField).ToList();

        await service.UpdateFieldMappingsAsync(
            templateId,
            new UpdateFieldMappingsRequest(mappings),
            CancellationToken.None);

        var activated = await service.ActivateAsync(templateId, CancellationToken.None);
        Assert.NotNull(activated);
        Assert.True(activated!.IsActive);
        Assert.Equal(fields.Count, activated.MappedFieldCount);
    }

    private static UpdateFieldMappingRequest MapField(Gdc.Modules.Plantillas.Domain.Entities.PdfTemplateField field) =>
        new(
            field.Id,
            ResolveFieldType(field.AcroformName, field.FieldType),
            MapAcroformToSystemVariable(field.AcroformName),
            field.AcroformName == "comparendo_estado"
                ? ["Pendiente", "Notificado", "En trámite", "Pagado", "Cerrado"]
                : null);

    private static string ResolveFieldType(string acroformName, string inferredType) =>
        acroformName switch
        {
            "comparendo_total" => PlantillaFieldTypes.Number,
            "comparendo_estado" => PlantillaFieldTypes.Choice,
            _ => inferredType,
        };

    private static string MapAcroformToSystemVariable(string acroformName) =>
        acroformName switch
        {
            "tenant_nombre" => "tenant.nombre",
            "comparendo_numero" => "comparendo.numero",
            "comparendo_placa" => "comparendo.placa",
            "comparendo_documento" => "comparendo.documento",
            "comparendo_infractor_nombre" => "comparendo.infractor_nombre",
            "comparendo_fecha_comparendo" => "comparendo.fecha_comparendo",
            "comparendo_fecha_notificacion" => "comparendo.fecha_notificacion",
            "comparendo_total" => "comparendo.total",
            "comparendo_estado" => "comparendo.estado",
            "contraventor_nombre" => "contraventor.nombre",
            "contraventor_documento" => "contraventor.documento",
            "contraventor_correo" => "contraventor.correo",
            "secretaria_destino" => "derecho_peticion.secretaria_destino",
            _ => "comparendo.numero",
        };

    private static async Task<Guid> UploadFixtureTemplate(GdcPlantillaService service)
    {
        await using var stream = OpenFixturePdf();
        var response = await service.UploadAsync(
            stream,
            "vialix-dp-template.pdf",
            "application/pdf",
            "DP Vialix",
            CancellationToken.None);

        return response.Id;
    }

    private static GdcPlantillaService CreateService(GdcDbContext db) =>
        new(
            db,
            new FakeTenantContext(TenantId),
            new PdfBinaryAssetStore(),
            new PdfSharpAcroFormFieldExtractor(),
            TimeProvider.System);

    private static GdcDbContext CreateDbContext()
    {
        PdfBinaryAssetStore.Clear();
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GdcDbContext(options, new TestSupport.TestTenantContext());
    }

    private static FileStream OpenFixturePdf()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "fixtures", "pdf", "vialix-dp-template.pdf"));

        return File.OpenRead(path);
    }

    private sealed class FakeTenantContext(Guid tenantId) : DgcTenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }
}
