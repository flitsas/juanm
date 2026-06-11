using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Rules;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Notif;

public sealed class NotifSwitchService(
    GdcDbContext db,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
{
    public async Task<SwitchResponse> GetAsync(CancellationToken cancellationToken)
    {
        var enabled = await db.EmailProviderConfigs
            .Where(p => p.TenantId == tenantContext.TenantId && p.IsActive && p.DeletedAt == null)
            .Select(p => (bool?)p.DispatchEnabled)
            .FirstOrDefaultAsync(cancellationToken);

        return new SwitchResponse(enabled ?? true);
    }

    public async Task<SwitchResponse> UpdateAsync(UpdateSwitchRequest request, CancellationToken cancellationToken)
    {
        var configs = await db.EmailProviderConfigs
            .Where(p => p.TenantId == tenantContext.TenantId && p.IsActive && p.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (configs.Count == 0)
        {
            throw new InvalidOperationException("No active provider configuration found.");
        }

        var now = timeProvider.GetUtcNow();
        foreach (var config in configs)
        {
            config.DispatchEnabled = request.DispatchEnabled;
            config.UpdatedAt = now;
            config.UpdatedBy = tenantContext.UserId;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new SwitchResponse(request.DispatchEnabled);
    }
}
