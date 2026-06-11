using System.Net.Http.Json;
using System.Text.Json;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Application.Ocr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Dgc;

public sealed class HttpOcrExtractor(
    HttpClient httpClient,
    IOptions<DgcOcrOptions> options,
    ILogger<HttpOcrExtractor> logger) : IOcrExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OcrExtractedFields> ExtractAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

            var response = await httpClient.PostAsync(
                $"{options.Value.PythonMlBaseUrl.TrimEnd('/')}/ocr/comparendo:extract",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "OCR service returned {Status} for {FileName}: {Body}",
                    (int)response.StatusCode,
                    fileName,
                    body);
                return EmptyWithMessage($"python-ml HTTP {(int)response.StatusCode}");
            }

            var payload = await response.Content.ReadFromJsonAsync<OcrApiResponse>(JsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("Empty OCR response from python-ml.");

            return new OcrExtractedFields(
                payload.NumeroComparendo,
                payload.Placa,
                payload.Documento,
                payload.Infractor,
                payload.Fecha,
                payload.Valor,
                payload.Infraccion,
                payload.Confidence,
                payload.RawText ?? string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OCR service unavailable for {FileName}, returning empty extraction.", fileName);
            return EmptyWithMessage(ex.Message);
        }
    }

    private static OcrExtractedFields EmptyWithMessage(string message) =>
        new(null, null, null, null, null, null, null, 0, message);

    private sealed record OcrApiResponse(
        string? NumeroComparendo,
        string? Placa,
        string? Documento,
        string? Infractor,
        string? Fecha,
        decimal? Valor,
        string? Infraccion,
        double Confidence,
        string? RawText);
}

public sealed class DgcOcrOptions
{
    public const string SectionName = "Dgc";

    public string PythonMlBaseUrl { get; set; } = "http://localhost:4012";
}
