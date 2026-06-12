using System.Text;
using Gdc.Modules.Reglas.Application.Abstractions;

namespace Gdc.Infrastructure.Reglas;

public sealed class GdcPdfTemplateRendererStub : IGdcPdfTemplateRenderer
{
    public Task<PdfRenderResult> RenderAsync(
        Guid templateId,
        IReadOnlyDictionary<string, string> tags,
        CancellationToken cancellationToken)
    {
        var numero = tags.GetValueOrDefault("numero_comparendo", "N/A");
        var content = Encoding.UTF8.GetBytes(
            $"""
            %PDF-1.4
            1 0 obj<<>>endobj
            trailer<<>>
            %% FLIT REGLAS stub template={templateId:N} comparendo={numero}
            """);

        var documentRef = $"reglas/stub/{templateId:N}/{numero}.pdf";
        return Task.FromResult(new PdfRenderResult(content, documentRef));
    }
}
