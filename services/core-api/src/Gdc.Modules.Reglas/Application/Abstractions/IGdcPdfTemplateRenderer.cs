namespace Gdc.Modules.Reglas.Application.Abstractions;

public sealed record PdfRenderResult(byte[] Content, string DocumentRef);

public interface IGdcPdfTemplateRenderer
{
    Task<PdfRenderResult> RenderAsync(
        Guid templateId,
        IReadOnlyDictionary<string, string> tags,
        CancellationToken cancellationToken);
}
