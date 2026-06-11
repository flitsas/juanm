using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Notif.Application.Rules;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Notif;

public sealed class NotifQueueService(GdcDbContext db, DgcTenantContext tenantContext)
{
    public async Task<QueueListResponse> ListAsync(CancellationToken cancellationToken)
    {
        var items = await db.EmailQueues
            .Where(q => q.TenantId == tenantContext.TenantId && q.DeletedAt == null)
            .OrderByDescending(q => q.ScheduledAt)
            .Select(q => new QueueItemResponse(
                q.Id,
                q.NotificationRuleId,
                q.EmailTemplateId,
                q.ComparendoId,
                q.Destino,
                q.Status,
                q.ScheduledAt,
                q.ProcessedAt,
                q.ErrorMessage))
            .ToListAsync(cancellationToken);

        return new QueueListResponse(items);
    }
}
