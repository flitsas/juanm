namespace Gdc.Modules.Dgc.Application.Ocr;

public sealed record OcrExtractedFields(
    string? NumeroComparendo,
    string? Placa,
    string? Documento,
    string? Infractor,
    string? Fecha,
    decimal? Valor,
    string? Infraccion,
    double Confidence,
    string RawText);
