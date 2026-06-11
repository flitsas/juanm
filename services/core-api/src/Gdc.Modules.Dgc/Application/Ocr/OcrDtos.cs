namespace Gdc.Modules.Dgc.Application.Ocr;

public sealed record OcrUploadFile(string FileName, string ContentType, Stream Content);

public sealed record OcrLoteUploadResult(Guid LoteId, IReadOnlyList<OcrItemResponse> Items);

public sealed record OcrItemResponse(
    Guid ItemId,
    Guid LoteId,
    string Estado,
    string ArchivoUri,
    OcrExtractedFields Fields);

public sealed record ConfirmOcrItemRequest(
    string NumeroComparendo,
    string Estado,
    string? InfractorNombre,
    string? Documento,
    string? Placa,
    string? InfraccionCodigo,
    DateOnly? FechaComparendo,
    decimal TotalValor);

public sealed record ConfirmOcrItemResult(
    bool Success,
    Guid? ComparendoId,
    string? ErrorCode,
    string? ErrorMessage);
