using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Plantillas;
using Gdc.Modules.Plantillas.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Reglas;

/// <summary>
/// Integración REGLAS (#9710) → GDC-PLANTILLAS (#9564): compila PDF AcroForm real.
/// </summary>
public sealed class GdcPdfTemplateRendererAdapter(
    GdcDbContext db,
    ITenantContext tenantContext,
    IComparendoCompilationSource compilationSource,
    IBinaryAssetStore binaryAssetStore,
    IPdfCompiler pdfCompiler) : IGdcPdfTemplateRenderer
{
    public async Task<PdfRenderResult> RenderAsync(
        Guid templateId,
        IReadOnlyDictionary<string, string> tags,
        CancellationToken cancellationToken)
    {
        if (!tags.TryGetValue("comparendo_id", out var comparendoRaw)
            || !Guid.TryParse(comparendoRaw, out var comparendoId))
        {
            throw new ArgumentException("Tag 'comparendo_id' is required for PDF rendering.");
        }

        var tenantId = tenantContext.TenantId
            ?? throw new InvalidOperationException("Tenant context is required for PDF rendering.");

        var template = await db.PdfTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(
                t => t.Id == templateId && t.TenantId == tenantId && t.DeletedAt == null,
                cancellationToken)
            ?? throw new ArgumentException($"PDF template {templateId} was not found.");

        GdcPdfCompilationHelper.EnsureTemplateReadyForCompilation(template);

        var snapshot = await compilationSource.GetByIdAsync(comparendoId, tenantId, cancellationToken)
            ?? throw new ArgumentException($"Comparendo {comparendoId} was not found for PDF compilation.");

        var storageKey = await GdcPdfCompilationHelper.CompileAndStoreAsync(
            template,
            snapshot,
            tenantId,
            comparendoId,
            binaryAssetStore,
            pdfCompiler,
            cancellationToken);

        var pdfBytes = await binaryAssetStore.GetAsync(tenantId, storageKey, cancellationToken)
            ?? throw new InvalidOperationException("Compiled PDF could not be loaded from storage.");

        return new PdfRenderResult(pdfBytes, storageKey);
    }
}
