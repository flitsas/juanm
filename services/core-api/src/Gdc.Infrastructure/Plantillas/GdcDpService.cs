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
    private const string DownloadPathPrefix = "/api/v1/gdc/derechos-peticion";

    public async Task<DerechoPeticionListResponse> ListByComparendoAsync(
        Guid comparendoId,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var items = await db.DerechosPeticion
            .AsNoTracking()
            .Where(d => d.ComparendoId == comparendoId && d.TenantId == tenantId && d.DeletedAt == null)
            .OrderByDescending(d => d.GeneratedAt)
            .Select(d => new DerechoPeticionListItemDto(
                d.Id,
                d.ComparendoId,
                d.PdfTemplateId,
                d.TemplateVersion,
                d.Estado,
                d.GeneratedAt,
                $"{DownloadPathPrefix}/{d.Id}/download"))
            .ToListAsync(cancellationToken);

        return new DerechoPeticionListResponse(items);
    }

    public async Task<GenerateDerechoPeticionResponse?> GenerateAsync(
        Guid comparendoId,
        GenerateDerechoPeticionRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var snapshot = await RequireComparendoSnapshotAsync(comparendoId, tenantId, cancellationToken);
        if (snapshot is null)
        {
            return null;
        }

        EnsureContraventorPresent(comparendoId, tenantId, snapshot);

        var template = await db.PdfTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(
                t => t.Id == request.TemplateId && t.TenantId == tenantId && t.DeletedAt == null,
                cancellationToken);

        if (template is null)
        {
            throw new ArgumentException($"Template {request.TemplateId} not found.");
        }

        GdcPdfCompilationHelper.EnsureTemplateReadyForCompilation(template);

        var outputKey = await GdcPdfCompilationHelper.CompileAndStoreAsync(
            template,
            snapshot,
            tenantId,
            comparendoId,
            binaryAssetStore,
            pdfCompiler,
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

    public async Task<TransitionDerechoPeticionEstadoResponse?> TransitionEstadoAsync(
        Guid derechoPeticionId,
        TransitionDerechoPeticionEstadoRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var dp = await db.DerechosPeticion
            .FirstOrDefaultAsync(
                d => d.Id == derechoPeticionId && d.TenantId == tenantId && d.DeletedAt == null,
                cancellationToken);

        if (dp is null)
        {
            return null;
        }

        if (!DerechoPeticionEstadoTransitions.IsValidState(request.Estado))
        {
            throw new ArgumentException($"Invalid target estado '{request.Estado}'.");
        }

        if (!DerechoPeticionEstadoTransitions.CanTransition(dp.Estado, request.Estado))
        {
            throw new InvalidOperationException("GDC_DP_INVALID_TRANSITION");
        }

        var now = timeProvider.GetUtcNow();
        dp.Estado = request.Estado;
        dp.UpdatedAt = now;
        dp.UpdatedBy = tenantContext.UserId;
        await db.SaveChangesAsync(cancellationToken);

        return new TransitionDerechoPeticionEstadoResponse(dp.Id, dp.Estado, dp.UpdatedAt);
    }

    public async Task<(byte[] Pdf, string FileName)?> GetDownloadAsync(
        Guid derechoPeticionId,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var dp = await db.DerechosPeticion
            .AsNoTracking()
            .FirstOrDefaultAsync(
                d => d.Id == derechoPeticionId && d.TenantId == tenantId && d.DeletedAt == null,
                cancellationToken);

        if (dp is null)
        {
            return null;
        }

        var pdf = await binaryAssetStore.GetAsync(tenantId, dp.OutputStorageKey, cancellationToken);
        if (pdf is null)
        {
            throw new InvalidOperationException("Generated PDF could not be loaded from storage.");
        }

        return (pdf, $"derecho-peticion-{dp.Id:N}.pdf");
    }

    public async Task<RegenerateNoEnviadoDpsResponse> RegenerateNoEnviadoDpsForTemplateAsync(
        PdfTemplate template,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var pending = await db.DerechosPeticion
            .Where(d =>
                d.PdfTemplateId == template.Id
                && d.TenantId == tenantId
                && d.Estado == DerechoPeticionEstados.NoEnviado
                && d.DeletedAt == null)
            .ToListAsync(cancellationToken);

        var regenerated = 0;
        var now = timeProvider.GetUtcNow();

        foreach (var dp in pending)
        {
            var snapshot = await compilationSource.GetByIdAsync(dp.ComparendoId, tenantId, cancellationToken);
            if (snapshot is null || snapshot.PendienteContraventor || snapshot.Contraventor is null)
            {
                logger.LogWarning(
                    "Skipping DP {DpId} regeneration — comparendo {ComparendoId} not eligible",
                    dp.Id,
                    dp.ComparendoId);
                continue;
            }

            var outputKey = await GdcPdfCompilationHelper.CompileAndStoreAsync(
                template,
                snapshot,
                tenantId,
                dp.ComparendoId,
                binaryAssetStore,
                pdfCompiler,
                cancellationToken);

            dp.OutputStorageKey = outputKey;
            dp.TemplateVersion = template.Version;
            dp.GeneratedAt = now;
            dp.UpdatedAt = now;
            dp.UpdatedBy = tenantContext.UserId;
            regenerated++;
        }

        if (regenerated > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return new RegenerateNoEnviadoDpsResponse(template.Version, regenerated);
    }

    private async Task<ComparendoCompilationSnapshot?> RequireComparendoSnapshotAsync(
        Guid comparendoId,
        Guid tenantId,
        CancellationToken cancellationToken) =>
        await compilationSource.GetByIdAsync(comparendoId, tenantId, cancellationToken);

    private void EnsureContraventorPresent(
        Guid comparendoId,
        Guid tenantId,
        ComparendoCompilationSnapshot snapshot)
    {
        if (snapshot.PendienteContraventor || snapshot.Contraventor is null)
        {
            logger.LogWarning(
                "GDC_CONTRAVENTOR_REQUIRED: comparendo {ComparendoId} tenant {TenantId} blocked DP generation",
                comparendoId,
                tenantId);
            throw new InvalidOperationException("GDC_CONTRAVENTOR_REQUIRED");
        }
    }
}
