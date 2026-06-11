using System.Text.Json;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Dgc.Application.Ocr;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Dgc;

public sealed class OcrIngestionService(
    GdcDbContext db,
    IOcrExtractor ocrExtractor,
    DgcTenantContext tenantContext)
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg",
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OcrLoteUploadResult> UploadLoteAsync(
        IReadOnlyList<OcrUploadFile> files,
        CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.", nameof(files));
        }

        var tenantId = tenantContext.TenantId;
        var now = DateTimeOffset.UtcNow;
        var lote = new OcrLote
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Estado = "processing",
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.OcrLotes.Add(lote);
        var items = new List<OcrItemResponse>();

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Unsupported file type: {extension}");
            }

            await using var stream = file.Content;
            var extracted = await ocrExtractor.ExtractAsync(stream, file.FileName, file.ContentType, cancellationToken);
            var payloadJson = JsonSerializer.Serialize(extracted, JsonOptions);

            var item = new OcrItem
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantId,
                OcrLoteId = lote.Id,
                ArchivoUri = $"upload://{lote.Id}/{file.FileName}",
                Estado = "pending",
                OcrPayloadJson = payloadJson,
                CreatedAt = now,
                CreatedBy = tenantContext.UserId,
            };

            db.OcrItems.Add(item);
            items.Add(MapItem(item, extracted));
        }

        lote.Estado = "ready";
        await db.SaveChangesAsync(cancellationToken);

        return new OcrLoteUploadResult(lote.Id, items);
    }

    public async Task<IReadOnlyList<OcrItemResponse>> GetLoteItemsAsync(
        Guid loteId,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var items = await db.OcrItems
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.OcrLoteId == loteId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(i => MapItem(i, DeserializePayload(i.OcrPayloadJson))).ToList();
    }

    public async Task<ConfirmOcrItemResult> ConfirmItemAsync(
        Guid itemId,
        ConfirmOcrItemRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var item = await db.OcrItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.TenantId == tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"OCR item {itemId} was not found.");

        if (item.Estado is "confirmed" or "duplicate")
        {
            throw new InvalidOperationException($"OCR item {itemId} is already {item.Estado}.");
        }

        var numero = request.NumeroComparendo.Trim();
        var exists = await db.Comparendos
            .AsNoTracking()
            .AnyAsync(
                c => c.TenantId == tenantId && c.NumeroComparendo == numero,
                cancellationToken);

        if (exists)
        {
            item.Estado = "duplicate";
            await db.SaveChangesAsync(cancellationToken);
            return new ConfirmOcrItemResult(false, null, "DGC_DUPLICATE", "Comparendo number already exists for tenant.");
        }

        var now = DateTimeOffset.UtcNow;
        var comparendo = new Comparendo
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            NumeroComparendo = numero,
            Estado = request.Estado.Trim(),
            InfractorNombre = request.InfractorNombre?.Trim(),
            Documento = request.Documento?.Trim(),
            Placa = request.Placa?.Trim(),
            InfraccionCodigo = request.InfraccionCodigo?.Trim(),
            FechaComparendo = request.FechaComparendo,
            TotalValor = request.TotalValor,
            Fuente = "ocr",
            PendienteContraventor = true,
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.Comparendos.Add(comparendo);
        item.ComparendoId = comparendo.Id;
        item.Estado = "confirmed";
        item.UpdatedAt = now;
        item.UpdatedBy = tenantContext.UserId;

        await db.SaveChangesAsync(cancellationToken);

        return new ConfirmOcrItemResult(true, comparendo.Id, null, null);
    }

    private static OcrItemResponse MapItem(OcrItem item, OcrExtractedFields fields) =>
        new(
            item.Id,
            item.OcrLoteId,
            item.Estado,
            item.ArchivoUri,
            fields);

    private static OcrExtractedFields DeserializePayload(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new OcrExtractedFields(null, null, null, null, null, null, null, 0, string.Empty);
        }

        return JsonSerializer.Deserialize<OcrExtractedFields>(json, JsonOptions)
            ?? new OcrExtractedFields(null, null, null, null, null, null, null, 0, string.Empty);
    }
}
