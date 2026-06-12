using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Plantillas.Application;
using Gdc.Modules.Plantillas.Application.Templates;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Plantillas;

/// <summary>
/// Uso de ejemplo:
/// var dpService = PlantillasTestHelpers.CreateDpService(db);
/// await dpService.TransitionEstadoAsync(dpId, new TransitionDerechoPeticionEstadoRequest("Enviado"), ct);
/// </summary>
[Collection("Plantillas")]
public sealed class GdcDpLifecycleTests
{
    [Fact]
    public async Task ListByComparendoAsync_returns_multidocument_grid_with_download_path()
    {
        await using var db = PlantillasTestHelpers.CreateDbContext();
        var comparendoId = PlantillasTestHelpers.SeedComparendoWithContraventor(db);
        await db.SaveChangesAsync();

        var plantillaService = PlantillasTestHelpers.CreatePlantillaService(db);
        var templateId = await PlantillasTestHelpers.UploadAndActivateTemplateAsync(plantillaService, db);
        var dpService = PlantillasTestHelpers.CreateDpService(db);

        await dpService.GenerateAsync(comparendoId, new GenerateDerechoPeticionRequest(templateId), CancellationToken.None);
        await dpService.GenerateAsync(comparendoId, new GenerateDerechoPeticionRequest(templateId), CancellationToken.None);

        var list = await dpService.ListByComparendoAsync(comparendoId, CancellationToken.None);
        Assert.Equal(2, list.Items.Count);
        Assert.All(list.Items, item =>
        {
            Assert.Equal(comparendoId, item.ComparendoId);
            Assert.Contains("/download", item.DownloadPath);
        });
    }

    [Fact]
    public async Task TransitionEstadoAsync_follows_rf05_sequence()
    {
        await using var db = PlantillasTestHelpers.CreateDbContext();
        var comparendoId = PlantillasTestHelpers.SeedComparendoWithContraventor(db);
        await db.SaveChangesAsync();

        var plantillaService = PlantillasTestHelpers.CreatePlantillaService(db);
        var templateId = await PlantillasTestHelpers.UploadAndActivateTemplateAsync(plantillaService, db);
        var dpService = PlantillasTestHelpers.CreateDpService(db);

        var generated = await dpService.GenerateAsync(
            comparendoId,
            new GenerateDerechoPeticionRequest(templateId),
            CancellationToken.None);

        var dpId = generated!.DerechoPeticionId;

        await dpService.TransitionEstadoAsync(
            dpId,
            new TransitionDerechoPeticionEstadoRequest(DerechoPeticionEstados.Enviado),
            CancellationToken.None);
        await dpService.TransitionEstadoAsync(
            dpId,
            new TransitionDerechoPeticionEstadoRequest(DerechoPeticionEstados.SinRespuesta),
            CancellationToken.None);
        var finalState = await dpService.TransitionEstadoAsync(
            dpId,
            new TransitionDerechoPeticionEstadoRequest(DerechoPeticionEstados.ConRespuesta),
            CancellationToken.None);

        Assert.Equal(DerechoPeticionEstados.ConRespuesta, finalState!.Estado);
    }

    [Fact]
    public async Task TransitionEstadoAsync_rejects_backward_transition_from_con_respuesta()
    {
        await using var db = PlantillasTestHelpers.CreateDbContext();
        var comparendoId = PlantillasTestHelpers.SeedComparendoWithContraventor(db);
        await db.SaveChangesAsync();

        var plantillaService = PlantillasTestHelpers.CreatePlantillaService(db);
        var templateId = await PlantillasTestHelpers.UploadAndActivateTemplateAsync(plantillaService, db);
        var dpService = PlantillasTestHelpers.CreateDpService(db);

        var generated = await dpService.GenerateAsync(
            comparendoId,
            new GenerateDerechoPeticionRequest(templateId),
            CancellationToken.None);
        var dpId = generated!.DerechoPeticionId;

        foreach (var estado in new[]
        {
            DerechoPeticionEstados.Enviado,
            DerechoPeticionEstados.SinRespuesta,
            DerechoPeticionEstados.ConRespuesta,
        })
        {
            await dpService.TransitionEstadoAsync(
                dpId,
                new TransitionDerechoPeticionEstadoRequest(estado),
                CancellationToken.None);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dpService.TransitionEstadoAsync(
                dpId,
                new TransitionDerechoPeticionEstadoRequest(DerechoPeticionEstados.NoEnviado),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetDownloadAsync_returns_compiled_pdf_bytes()
    {
        await using var db = PlantillasTestHelpers.CreateDbContext();
        var comparendoId = PlantillasTestHelpers.SeedComparendoWithContraventor(db);
        await db.SaveChangesAsync();

        var plantillaService = PlantillasTestHelpers.CreatePlantillaService(db);
        var templateId = await PlantillasTestHelpers.UploadAndActivateTemplateAsync(plantillaService, db);
        var dpService = PlantillasTestHelpers.CreateDpService(db);

        var generated = await dpService.GenerateAsync(
            comparendoId,
            new GenerateDerechoPeticionRequest(templateId),
            CancellationToken.None);

        var download = await dpService.GetDownloadAsync(generated!.DerechoPeticionId, CancellationToken.None);
        Assert.NotNull(download);
        Assert.True(download!.Value.Pdf.Length > 1000);
        Assert.EndsWith(".pdf", download.Value.FileName);
    }

    [Fact]
    public async Task UpdateFieldMappingsAsync_regenerates_only_no_enviado_dps_on_template_bump()
    {
        await using var db = PlantillasTestHelpers.CreateDbContext();
        var comparendoId = PlantillasTestHelpers.SeedComparendoWithContraventor(db);
        await db.SaveChangesAsync();

        var plantillaService = PlantillasTestHelpers.CreatePlantillaService(db);
        var templateId = await PlantillasTestHelpers.UploadAndActivateTemplateAsync(plantillaService, db);
        var dpService = PlantillasTestHelpers.CreateDpService(db);

        var noEnviado = await dpService.GenerateAsync(
            comparendoId,
            new GenerateDerechoPeticionRequest(templateId),
            CancellationToken.None);
        var enviado = await dpService.GenerateAsync(
            comparendoId,
            new GenerateDerechoPeticionRequest(templateId),
            CancellationToken.None);

        await dpService.TransitionEstadoAsync(
            enviado!.DerechoPeticionId,
            new TransitionDerechoPeticionEstadoRequest(DerechoPeticionEstados.Enviado),
            CancellationToken.None);

        var noEnviadoEntity = await db.DerechosPeticion.SingleAsync(d => d.Id == noEnviado!.DerechoPeticionId);
        var enviadoEntity = await db.DerechosPeticion.SingleAsync(d => d.Id == enviado.DerechoPeticionId);
        var oldNoEnviadoKey = noEnviadoEntity.OutputStorageKey;
        var oldEnviadoKey = enviadoEntity.OutputStorageKey;
        var versionBefore = await db.PdfTemplates.Where(t => t.Id == templateId).Select(t => t.Version).SingleAsync();

        var fields = await db.PdfTemplateFields.Where(f => f.PdfTemplateId == templateId).ToListAsync();
        var numero = fields.First(f => f.AcroformName == "comparendo_numero");
        await plantillaService.UpdateFieldMappingsAsync(
            templateId,
            new UpdateFieldMappingsRequest([
                new UpdateFieldMappingRequest(
                    numero.Id,
                    numero.FieldType,
                    "comparendo.numero",
                    null),
            ]),
            CancellationToken.None);

        var templateAfter = await db.PdfTemplates.SingleAsync(t => t.Id == templateId);
        Assert.Equal(versionBefore + 1, templateAfter.Version);

        noEnviadoEntity = await db.DerechosPeticion.SingleAsync(d => d.Id == noEnviado!.DerechoPeticionId);
        enviadoEntity = await db.DerechosPeticion.SingleAsync(d => d.Id == enviado.DerechoPeticionId);

        Assert.Equal(templateAfter.Version, noEnviadoEntity.TemplateVersion);
        Assert.NotEqual(oldNoEnviadoKey, noEnviadoEntity.OutputStorageKey);
        Assert.Equal(versionBefore, enviadoEntity.TemplateVersion);
        Assert.Equal(oldEnviadoKey, enviadoEntity.OutputStorageKey);
    }
}
