using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Plantillas.Application.Abstractions;
using Gdc.Modules.Plantillas.Application.Templates;
using Gdc.Modules.Plantillas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Plantillas;

public sealed class GdcPlantillaService(
    GdcDbContext db,
    DgcTenantContext tenantContext,
    IBinaryAssetStore binaryAssetStore,
    IAcroFormFieldExtractor acroFormExtractor,
    TimeProvider timeProvider)
{
    private static readonly HashSet<string> AcceptedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
    };

    public async Task<PdfTemplateListResponse> ListAsync(CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var items = await db.PdfTemplates
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.Name)
            .Select(t => new PdfTemplateSummaryDto(
                t.Id,
                t.Name,
                t.Version,
                t.IsActive,
                t.Fields.Count))
            .ToListAsync(cancellationToken);

        return new PdfTemplateListResponse(items);
    }

    public async Task<PdfTemplateDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var template = await db.PdfTemplates
            .Include(t => t.Fields.OrderBy(f => f.SortOrder))
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, cancellationToken);

        return template is null ? null : ToDetailDto(template);
    }

    public async Task<UploadPdfTemplateResponse> UploadAsync(
        Stream content,
        string fileName,
        string? contentType,
        string? name,
        CancellationToken cancellationToken)
    {
        ValidatePdfUpload(fileName, contentType);

        var tenantId = tenantContext.TenantId;
        var templateName = string.IsNullOrWhiteSpace(name)
            ? Path.GetFileNameWithoutExtension(fileName).Trim()
            : name.Trim();

        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new ArgumentException("Template name is required.");
        }

        var storageKey = await binaryAssetStore.SaveAsync(
            tenantId,
            "templates",
            content,
            fileName,
            cancellationToken);

        await using var extractStream = new MemoryStream(
            await binaryAssetStore.GetAsync(tenantId, storageKey, cancellationToken)
            ?? throw new InvalidOperationException("Uploaded PDF could not be read back from storage."));

        var detected = acroFormExtractor.ExtractFields(extractStream);
        if (detected.Count == 0)
        {
            throw new ArgumentException("No AcroForm fields were detected in the PDF.");
        }

        var now = timeProvider.GetUtcNow();
        var template = new PdfTemplate
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = templateName,
            StorageKey = storageKey,
            Version = 1,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        var sortOrder = 0;
        foreach (var field in detected)
        {
            template.Fields.Add(new PdfTemplateField
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantId,
                PdfTemplateId = template.Id,
                AcroformName = field.AcroformName,
                FieldType = field.FieldType,
                SortOrder = sortOrder++,
                CreatedAt = now,
                CreatedBy = tenantContext.UserId,
            });
        }

        db.PdfTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);

        return new UploadPdfTemplateResponse(
            template.Id,
            template.Name,
            template.Version,
            template.Fields
                .OrderBy(f => f.SortOrder)
                .Select(ToFieldDto)
                .ToList());
    }

    private static void ValidatePdfUpload(string fileName, string? contentType)
    {
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only PDF files are accepted.");
        }

        if (contentType is not null
            && !AcceptedContentTypes.Contains(contentType)
            && !contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Content-Type must be application/pdf.");
        }
    }

    private static PdfTemplateFieldDto ToFieldDto(PdfTemplateField field) =>
        new(field.Id, field.AcroformName, field.FieldType, field.SystemVariable, field.SortOrder);

    private static PdfTemplateDetailDto ToDetailDto(PdfTemplate template) =>
        new(
            template.Id,
            template.Name,
            template.Version,
            template.IsActive,
            template.Description,
            template.Fields.OrderBy(f => f.SortOrder).Select(ToFieldDto).ToList());
}
