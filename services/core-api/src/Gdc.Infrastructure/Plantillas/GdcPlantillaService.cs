using System.Text.Json;
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
    GdcDpService dpService,
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
            IsActive = false,
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

#pragma warning disable CA1822 // Servicio inyectable por DI; la instancia es intencional.
    public SystemVariableListResponse GetSystemVariables() =>
        new(SystemVariableCatalog.All
            .Select(v => new SystemVariableDto(v.Key, v.Label, v.Source, v.DataType))
            .ToList());
#pragma warning restore CA1822

    public async Task<PdfTemplateDetailDto?> UpdateFieldMappingsAsync(
        Guid templateId,
        UpdateFieldMappingsRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var template = await db.PdfTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Id == templateId && t.TenantId == tenantId, cancellationToken);

        if (template is null)
        {
            return null;
        }

        var fieldLookup = template.Fields.ToDictionary(f => f.Id);
        foreach (var mapping in request.Fields)
        {
            if (!fieldLookup.TryGetValue(mapping.FieldId, out var field))
            {
                throw new ArgumentException($"Field {mapping.FieldId} does not belong to template {templateId}.");
            }

            PlantillaFieldMappingValidator.ValidateMapping(
                mapping.FieldType,
                mapping.SystemVariable,
                mapping.ChoiceOptions,
                out var choiceOptionsJson);

            field.FieldType = mapping.FieldType;
            field.SystemVariable = mapping.SystemVariable;
            field.ChoiceOptionsJson = choiceOptionsJson;
            field.UpdatedAt = timeProvider.GetUtcNow();
            field.UpdatedBy = tenantContext.UserId;
        }

        var bumpVersion = template.IsActive;
        if (bumpVersion)
        {
            template.Version++;
            template.UpdatedAt = timeProvider.GetUtcNow();
            template.UpdatedBy = tenantContext.UserId;
        }

        await db.SaveChangesAsync(cancellationToken);

        if (bumpVersion)
        {
            GdcPdfCompilationHelper.EnsureTemplateReadyForCompilation(template);
            await dpService.RegenerateNoEnviadoDpsForTemplateAsync(template, cancellationToken);
        }

        return ToDetailDto(template);
    }

    public async Task<ActivateTemplateResponse?> ActivateAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var template = await db.PdfTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Id == templateId && t.TenantId == tenantId, cancellationToken);

        if (template is null)
        {
            return null;
        }

        var unmapped = template.Fields
            .Where(f => string.IsNullOrWhiteSpace(f.SystemVariable))
            .Select(f => f.AcroformName)
            .ToList();

        if (unmapped.Count > 0)
        {
            throw new ArgumentException(
                $"Template cannot be activated until all fields are mapped: {string.Join(", ", unmapped)}.");
        }

        template.IsActive = true;
        template.UpdatedAt = timeProvider.GetUtcNow();
        template.UpdatedBy = tenantContext.UserId;
        await db.SaveChangesAsync(cancellationToken);

        return new ActivateTemplateResponse(template.Id, template.IsActive, template.Fields.Count);
    }

    private static PdfTemplateFieldDto ToFieldDto(PdfTemplateField field) =>
        new(
            field.Id,
            field.AcroformName,
            field.FieldType,
            field.SystemVariable,
            ParseChoiceOptions(field.ChoiceOptionsJson),
            field.SortOrder);

    private static List<string>? ParseChoiceOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<string>>(json);
    }

    private static PdfTemplateDetailDto ToDetailDto(PdfTemplate template) =>
        new(
            template.Id,
            template.Name,
            template.Version,
            template.IsActive,
            template.Description,
            template.Fields.OrderBy(f => f.SortOrder).Select(ToFieldDto).ToList());
}
