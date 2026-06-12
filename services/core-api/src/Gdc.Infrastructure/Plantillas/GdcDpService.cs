using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Plantillas.Application;
using Gdc.Modules.Plantillas.Application.Abstractions;
using Gdc.Modules.Plantillas.Application.Templates;
using Gdc.Modules.Plantillas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gdc.Infrastructure.Plantillas;

public sealed class GdcDpService(
    GdcDbContext db,
    DgcTenantContext tenantContext,
    IComparendoCompilationSource compilationSource,
    IBinaryAssetStore binaryAssetStore,
    IPdfCompiler pdfCompiler,
    TimeProvider timeProvider,
    ILogger<GdcDpService> logger)
{
    public async Task<GenerateDerechoPeticionResponse?> GenerateAsync(
        Guid comparendoId,
        GenerateDerechoPeticionRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var snapshot = await compilationSource.GetByIdAsync(comparendoId, tenantId, cancellationToken);
        if (snapshot is null)
        {
            return null;
        }

        if (snapshot.PendienteContraventor || snapshot.Contraventor is null)
        {
            logger.LogWarning(
                "GDC_CONTRAVENTOR_REQUIRED: comparendo {ComparendoId} tenant {TenantId} blocked DP generation",
                comparendoId,
                tenantId);
            throw new InvalidOperationException("GDC_CONTRAVENTOR_REQUIRED");
        }

        var template = await db.PdfTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(
                t => t.Id == request.TemplateId && t.TenantId == tenantId && t.DeletedAt == null,
                cancellationToken);

        if (template is null)
        {
            throw new ArgumentException($"Template {request.TemplateId} not found.");
        }

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
        var outputKey = await binaryAssetStore.SaveAsync(
            tenantId,
            "derechos-peticion",
            compiledStream,
            $"{comparendoId:N}.pdf",
            cancellationToken);

        var now = timeProvider.GetUtcNow();
        var dp = new DerechoPeticion
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            ComparendoId = comparendoId,
            PdfTemplateId = template.Id,
            TemplateVersion = template.Version,
            Estado = DerechoPeticionEstados.NoEnviado,
            OutputStorageKey = outputKey,
            GeneratedAt = now,
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.DerechosPeticion.Add(dp);
        await db.SaveChangesAsync(cancellationToken);

        return new GenerateDerechoPeticionResponse(
            dp.Id,
            dp.ComparendoId,
            dp.PdfTemplateId,
            dp.TemplateVersion,
            dp.Estado,
            dp.OutputStorageKey,
            dp.GeneratedAt);
    }
}
