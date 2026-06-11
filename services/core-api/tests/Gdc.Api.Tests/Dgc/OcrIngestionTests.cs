using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Application.Ocr;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Dgc;

public sealed class OcrIngestionTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task UploadLoteAsync_creates_buffer_items_with_ocr_fields()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db, new FakeOcrExtractor());

        await using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);
        var result = await service.UploadLoteAsync(
            [new OcrUploadFile("comparendo.png", "image/png", stream)],
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("CMP-001", result.Items[0].Fields.NumeroComparendo);
        Assert.Equal("pending", result.Items[0].Estado);
        Assert.Equal(1, await db.OcrItems.CountAsync());
    }

    [Fact]
    public async Task ConfirmItemAsync_returns_duplicate_when_numero_exists()
    {
        await using var db = CreateDbContext();
        db.Comparendos.Add(new Comparendo
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            NumeroComparendo = "CMP-DUP",
            Estado = "Pendiente",
            Fuente = "manual",
            PendienteContraventor = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var loteId = Guid.CreateVersion7();
        var itemId = Guid.CreateVersion7();
        db.OcrLotes.Add(new OcrLote
        {
            Id = loteId,
            TenantId = TenantId,
            Estado = "ready",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.OcrItems.Add(new OcrItem
        {
            Id = itemId,
            TenantId = TenantId,
            OcrLoteId = loteId,
            ArchivoUri = "upload://x.png",
            Estado = "pending",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var service = CreateService(db, new FakeOcrExtractor());
        var confirm = await service.ConfirmItemAsync(
            itemId,
            new ConfirmOcrItemRequest("CMP-DUP", "Pendiente", null, null, null, null, null, 100m),
            CancellationToken.None);

        Assert.False(confirm.Success);
        Assert.Equal("DGC_DUPLICATE", confirm.ErrorCode);
        Assert.Equal(1, await db.Comparendos.CountAsync());
    }

    [Fact]
    public async Task ConfirmItemAsync_persists_comparendo_when_unique()
    {
        await using var db = CreateDbContext();
        var loteId = Guid.CreateVersion7();
        var itemId = Guid.CreateVersion7();
        db.OcrLotes.Add(new OcrLote
        {
            Id = loteId,
            TenantId = TenantId,
            Estado = "ready",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.OcrItems.Add(new OcrItem
        {
            Id = itemId,
            TenantId = TenantId,
            OcrLoteId = loteId,
            ArchivoUri = "upload://x.png",
            Estado = "pending",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var service = CreateService(db, new FakeOcrExtractor());
        var confirm = await service.ConfirmItemAsync(
            itemId,
            new ConfirmOcrItemRequest("CMP-NEW", "Pendiente", "Juan", "123", "ABC123", null, null, 250m),
            CancellationToken.None);

        Assert.True(confirm.Success);
        Assert.NotNull(confirm.ComparendoId);
        Assert.Equal(1, await db.Comparendos.CountAsync(c => c.NumeroComparendo == "CMP-NEW"));
    }

    private static OcrIngestionService CreateService(GdcDbContext db, IOcrExtractor extractor) =>
        new(db, extractor, new FakeTenantContext(TenantId));

    private static GdcDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GdcDbContext(options);
    }

    private sealed class FakeOcrExtractor : IOcrExtractor
    {
        public Task<OcrExtractedFields> ExtractAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OcrExtractedFields(
                "CMP-001",
                "ABC123",
                "1234567890",
                "Juan Perez",
                "2025-01-15",
                150000m,
                "C29",
                0.8,
                "raw"));
    }

    private sealed class FakeTenantContext(Guid tenantId) : Gdc.Modules.Dgc.Application.Abstractions.ITenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => null;
    }
}
