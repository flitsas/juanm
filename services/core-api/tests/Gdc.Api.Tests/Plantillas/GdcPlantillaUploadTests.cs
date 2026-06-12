using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Plantillas;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Plantillas.Application.Abstractions;
using Gdc.Modules.Plantillas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Plantillas;

[Collection("Plantillas")]
public sealed class GdcPlantillaUploadTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task UploadAsync_extracts_acroform_fields_from_vialix_fixture()
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

        Assert.Equal("DP Vialix", response.Name);
        Assert.True(response.DetectedFields.Count >= 13);
        Assert.Contains(response.DetectedFields, f => f.AcroformName == "comparendo_numero");

        var persisted = await db.PdfTemplateFields.CountAsync();
        Assert.Equal(response.DetectedFields.Count, persisted);
    }

    [Fact]
    public async Task UploadAsync_rejects_non_pdf_extension()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        using var stream = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadAsync(stream, "doc.txt", "text/plain", null, CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_rejects_pdf_without_acroform_fields()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        using var stream = CreateBlankPdf();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UploadAsync(stream, "blank.pdf", "application/pdf", null, CancellationToken.None));
    }

    [Fact]
    public void PdfSharp_extractor_reads_fixture_field_names()
    {
        var extractor = new PdfSharpAcroFormFieldExtractor();
        using var stream = OpenFixturePdf();
        var fields = extractor.ExtractFields(stream);

        Assert.Contains(fields, f => f.AcroformName == "tenant_nombre");
        Assert.Contains(fields, f => f.AcroformName == "comparendo_estado");
    }

    private static GdcPlantillaService CreateService(GdcDbContext db) =>
        PlantillasTestHelpers.CreatePlantillaService(db);

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

    private static MemoryStream CreateBlankPdf()
    {
        using var created = new PdfSharpCore.Pdf.PdfDocument();
        created.AddPage();
        var stream = new MemoryStream();
        created.Save(stream, false);
        stream.Position = 0;
        return stream;
    }

    private sealed class FakeTenantContext(Guid tenantId) : DgcTenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }
}
