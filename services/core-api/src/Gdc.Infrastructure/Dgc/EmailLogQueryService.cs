using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Dgc.Application.EmailLog;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Dgc;

public sealed class EmailLogQueryService(GdcDbContext db, DgcTenantContext tenantContext)
{
    public async Task<EmailLogListResult?> ListByComparendoAsync(
        Guid comparendoId,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        var comparendoExists = await db.Comparendos
            .AsNoTracking()
            .AnyAsync(c => c.Id == comparendoId && c.TenantId == tenantId, cancellationToken);

        if (!comparendoExists)
        {
            return null;
        }

        var items = await db.EmailLogs
            .AsNoTracking()
            .Where(e => e.ComparendoId == comparendoId && e.TenantId == tenantId)
            .OrderByDescending(e => e.SentAt)
            .Select(e => new EmailLogItemDto(
                e.Id,
                e.SentAt,
                e.Origen,
                e.Destino,
                e.Cc,
                e.TipoAlerta,
                e.EstadoEntrega))
            .ToListAsync(cancellationToken);

        return new EmailLogListResult(items);
    }

    public async Task<EmailEvidenceDto?> GetEvidenceAsync(
        Guid emailLogId,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        var log = await db.EmailLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == emailLogId && e.TenantId == tenantId, cancellationToken);

        if (log is null || string.IsNullOrWhiteSpace(log.HtmlEvidencia))
        {
            return null;
        }

        return new EmailEvidenceDto(log.Id, log.HtmlEvidencia);
    }
}
