using Gdc.Infrastructure.Plantillas;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Plantillas.Application;
using Gdc.Modules.Plantillas.Application.Templates;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf.AcroForms;
using PdfSharpCore.Pdf.IO;

namespace Gdc.Api.Tests.Plantillas;

/// <summary>
/// Uso de ejemplo:
/// var service = PlantillasTestHelpers.CreateDpService(db);
/// var response = await service.GenerateAsync(comparendoId, new GenerateDerechoPeticionRequest(templateId), ct);
/// </summary>
public sealed class GdcDpGenerationTests
{
    [Fact]
    public async Task GenerateAsync_compiles_pdf_and_creates_derecho_peticion_no_enviado()
    {
        await using var db = PlantillasTestHelpers.CreateDbContext();
        var comparendoId = PlantillasTestHelpers.SeedComparendoWithContraventor(db);
        await db.SaveChangesAsync();

        var plantillaService = PlantillasTestHelpers.CreatePlantillaService(db);
        var templateId = await PlantillasTestHelpers.UploadAndActivateTemplateAsync(plantillaService, db);

        var dpService = PlantillasTestHelpers.CreateDpService(db);
        var response = await dpService.GenerateAsync(
            comparendoId,
            new GenerateDerechoPeticionRequest(templateId),
            CancellationToken.None);

        Assert.Equal(comparendoId, response!.ComparendoId);
        Assert.Equal(templateId, response.TemplateId);
        Assert.Equal(DerechoPeticionEstados.NoEnviado, response.Estado);
        Assert.False(string.IsNullOrWhiteSpace(response.OutputStorageKey));

        var persisted = await db.DerechosPeticion.SingleAsync();
        Assert.Equal(response.DerechoPeticionId, persisted.Id);
        Assert.Equal(1, persisted.TemplateVersion);

        var pdfBytes = await new PdfBinaryAssetStore().GetAsync(
            PlantillasTestHelpers.TenantId,
            response.OutputStorageKey,
            CancellationToken.None);
        Assert.NotNull(pdfBytes);

        using var verifyStream = new MemoryStream(pdfBytes!);
        using var document = PdfReader.Open(verifyStream, PdfDocumentOpenMode.Import);
        var numero = (PdfTextField)document.AcroForm!.Fields["comparendo_numero"]!;
        Assert.Contains("CMP-9754", numero.Value?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task GenerateAsync_blocks_when_contraventor_missing()
    {
        await using var db = PlantillasTestHelpers.CreateDbContext();
        var comparendoId = PlantillasTestHelpers.SeedComparendoWithContraventor(
            db,
            "CMP-NO-CONT",
            pendienteContraventor: true,
            withContraventor: false);
        await db.SaveChangesAsync();

        var plantillaService = PlantillasTestHelpers.CreatePlantillaService(db);
        var templateId = await PlantillasTestHelpers.UploadAndActivateTemplateAsync(plantillaService, db);

        var dpService = PlantillasTestHelpers.CreateDpService(db);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dpService.GenerateAsync(
                comparendoId,
                new GenerateDerechoPeticionRequest(templateId),
                CancellationToken.None));

        Assert.Equal("GDC_CONTRAVENTOR_REQUIRED", ex.Message);
        Assert.Equal(0, await db.DerechosPeticion.CountAsync());
    }

    [Fact]
    public void ComparendoVariableValuesBuilder_maps_comparendo_and_contraventor_fields()
    {
        var snapshot = new Gdc.Modules.Plantillas.Application.Abstractions.ComparendoCompilationSnapshot(
            Guid.NewGuid(),
            false,
            "CMP-001",
            "Notificado",
            "Juan Perez",
            "1234567890",
            "ABC123",
            new DateOnly(2026, 5, 15),
            new DateOnly(2026, 5, 20),
            99.5m,
            new Gdc.Modules.Plantillas.Application.Abstractions.ContraventorCompilationSnapshot(
                "Maria Lopez",
                "55443322",
                "maria@flit.test"),
            "FLIT Tenant",
            "secretaria-guid");

        var values = ComparendoVariableValuesBuilder.Build(snapshot);

        Assert.Equal("CMP-001", values["comparendo.numero"]);
        Assert.Equal("Maria Lopez", values["contraventor.nombre"]);
        Assert.Equal("99.5", values["comparendo.total"]);
        Assert.Equal("FLIT Tenant", values["tenant.nombre"]);
    }
}
