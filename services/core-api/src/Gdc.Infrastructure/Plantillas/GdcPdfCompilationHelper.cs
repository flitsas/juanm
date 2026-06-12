using Gdc.Modules.Plantillas.Application.Abstractions;
using Gdc.Modules.Plantillas.Application.Templates;
using Gdc.Modules.Plantillas.Domain.Entities;

namespace Gdc.Infrastructure.Plantillas;

internal static class GdcPdfCompilationHelper
{
    internal static async Task<string> CompileAndStoreAsync(
        PdfTemplate template,
        ComparendoCompilationSnapshot snapshot,
        Guid tenantId,
        Guid comparendoId,
        IBinaryAssetStore binaryAssetStore,
        IPdfCompiler pdfCompiler,
        CancellationToken cancellationToken)
    {
        var variableValues = ComparendoVariableValuesBuilder.Build(snapshot);
        var acroformValues = new Dictionary<string, string>();
        foreach (var field in template.Fields)
        {
            var systemKey = field.SystemVariable!;
            if (!variableValues.TryGetValue(systemKey, out var value))
            {
                throw new ArgumentException($"No value resolved for system variable '{systemKey}'.");
            }

            acroformValues[field.AcroformName] = value;
        }

        var templatePdf = await binaryAssetStore.GetAsync(tenantId, template.StorageKey, cancellationToken);
        if (templatePdf is null)
        {
            throw new InvalidOperationException("Template PDF could not be loaded from storage.");
        }

        await using var templateStream = new MemoryStream(templatePdf);
        var compiledPdf = pdfCompiler.FillAcroFormFields(templateStream, acroformValues);
        await using var compiledStream = new MemoryStream(compiledPdf);
        return await binaryAssetStore.SaveAsync(
            tenantId,
            "derechos-peticion",
            compiledStream,
            $"{comparendoId:N}.pdf",
            cancellationToken);
    }

    internal static void EnsureTemplateReadyForCompilation(PdfTemplate template)
    {
        if (!template.IsActive)
        {
            throw new ArgumentException("Template must be active before generating a Derecho de Petición.");
        }

        var unmapped = template.Fields
            .Where(f => string.IsNullOrWhiteSpace(f.SystemVariable))
            .Select(f => f.AcroformName)
            .ToList();

        if (unmapped.Count > 0)
        {
            throw new ArgumentException(
                $"Template mapping incomplete: {string.Join(", ", unmapped)}.");
        }
    }
}
