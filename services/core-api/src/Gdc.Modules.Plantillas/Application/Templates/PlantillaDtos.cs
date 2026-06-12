namespace Gdc.Modules.Plantillas.Application.Templates;

public sealed record PdfTemplateFieldDto(
    Guid Id,
    string AcroformName,
    string FieldType,
    string? SystemVariable,
    int SortOrder);

public sealed record PdfTemplateSummaryDto(
    Guid Id,
    string Name,
    int Version,
    bool IsActive,
    int FieldCount);

public sealed record PdfTemplateDetailDto(
    Guid Id,
    string Name,
    int Version,
    bool IsActive,
    string? Description,
    IReadOnlyList<PdfTemplateFieldDto> Fields);

public sealed record PdfTemplateListResponse(IReadOnlyList<PdfTemplateSummaryDto> Items);

public sealed record UploadPdfTemplateResponse(
    Guid Id,
    string Name,
    int Version,
    IReadOnlyList<PdfTemplateFieldDto> DetectedFields);
