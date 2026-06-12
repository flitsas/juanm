using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Plantillas;
using Microsoft.EntityFrameworkCore;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Notif.Domain.Entities;
using Gdc.Modules.Plantillas.Application;
using Gdc.Modules.Plantillas.Application.Templates;
using Gdc.Modules.Plantillas.Domain.Entities;

namespace Gdc.Api.Tests.Plantillas;

internal static class PlantillasTestHelpers
{
    internal static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal static GdcDbContext CreateDbContext()
    {
        PdfBinaryAssetStore.Clear();
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GdcDbContext(options, new TestSupport.TestTenantContext());
    }

    internal static GdcPlantillaService CreatePlantillaService(GdcDbContext db)
    {
        var dpService = CreateDpService(db);
        return new GdcPlantillaService(
            db,
            new FakeTenantContext(TenantId),
            new PdfBinaryAssetStore(),
            new PdfSharpAcroFormFieldExtractor(),
            dpService,
            TimeProvider.System);
    }

    internal static GdcDpService CreateDpService(GdcDbContext db) =>
        new(
            db,
            new FakeTenantContext(TenantId),
            new ComparendoCompilationSource(db),
            new PdfBinaryAssetStore(),
            new PdfSharpPdfCompiler(),
            TimeProvider.System,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<GdcDpService>.Instance);

    internal static async Task<Guid> UploadAndActivateTemplateAsync(GdcPlantillaService plantillaService, GdcDbContext db)
    {
        await using var stream = OpenFixturePdf();
        var uploaded = await plantillaService.UploadAsync(
            stream,
            "vialix-dp-template.pdf",
            "application/pdf",
            "DP Vialix",
            CancellationToken.None);

        var fields = await db.PdfTemplateFields
            .Where(f => f.PdfTemplateId == uploaded.Id)
            .ToListAsync();

        await plantillaService.UpdateFieldMappingsAsync(
            uploaded.Id,
            new UpdateFieldMappingsRequest(fields.Select(MapField).ToList()),
            CancellationToken.None);

        await plantillaService.ActivateAsync(uploaded.Id, CancellationToken.None);
        return uploaded.Id;
    }

    internal static Guid SeedComparendoWithContraventor(
        GdcDbContext db,
        string numero = "CMP-9754",
        bool pendienteContraventor = false,
        bool withContraventor = true)
    {
        db.TenantCompanies.Add(new TenantCompany
        {
            Id = TenantId,
            Name = "FLIT Test Tenant",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        var comparendoId = Guid.CreateVersion7();
        db.Comparendos.Add(new Comparendo
        {
            Id = comparendoId,
            TenantId = TenantId,
            NumeroComparendo = numero,
            Estado = "Notificado",
            InfractorNombre = "Juan Perez",
            Documento = "1234567890",
            Placa = "ABC123",
            FechaComparendo = new DateOnly(2026, 5, 15),
            FechaNotificacion = new DateOnly(2026, 5, 20),
            TotalValor = 150000.50m,
            SecretariaId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Fuente = "ocr",
            PendienteContraventor = pendienteContraventor,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        if (withContraventor)
        {
            db.Contraventors.Add(new Contraventor
            {
                Id = Guid.CreateVersion7(),
                TenantId = TenantId,
                ComparendoId = comparendoId,
                Nombre = "Maria Lopez",
                Documento = "55443322",
                Correo = "maria@flit.test",
                AsociacionAutomatica = true,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        return comparendoId;
    }

    internal static FileStream OpenFixturePdf()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "fixtures", "pdf", "vialix-dp-template.pdf"));

        return File.OpenRead(path);
    }

    private static UpdateFieldMappingRequest MapField(PdfTemplateField field) =>
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

    internal sealed class FakeTenantContext(Guid tenantId) : DgcTenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }
}
