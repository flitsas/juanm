using Gdc.Modules.Dgc.Application.Ocr;

namespace Gdc.Modules.Dgc.Application.Abstractions;

public interface IOcrExtractor
{
    Task<OcrExtractedFields> ExtractAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
