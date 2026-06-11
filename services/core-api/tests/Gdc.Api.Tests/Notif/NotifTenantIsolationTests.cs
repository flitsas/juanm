using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Tests;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Notif;

public sealed class NotifTenantIsolationTests
{
    [Fact]
    public async Task Query_filter_excludes_soft_deleted_provider_configs()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using var context = CreateInMemoryContext();

        context.EmailProviderConfigs.Add(new EmailProviderConfig
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA,
            ProviderType = "sendgrid",
            FromAddress = "a@tenant.test",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        context.EmailProviderConfigs.Add(new EmailProviderConfig
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB,
            ProviderType = "api",
            FromAddress = "b@tenant.test",
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = DateTimeOffset.UtcNow,
        });
        await context.SaveChangesAsync();

        var visible = await context.EmailProviderConfigs.ToListAsync();

        Assert.Single(visible);
        Assert.Equal(tenantA, visible[0].TenantId);
    }

    private static GdcDbContext CreateInMemoryContext() => TestDbContextFactory.CreateInMemory();
}
