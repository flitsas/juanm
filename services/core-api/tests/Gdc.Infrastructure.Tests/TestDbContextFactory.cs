using Gdc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Tests;

public static class TestDbContextFactory
{
    public static GdcDbContext CreateInMemory(ITenantContext? tenantContext = null, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"gdc-test-{Guid.NewGuid()}")
            .Options;

        var context = new GdcDbContext(options, tenantContext ?? new TenantContext());
        context.Database.EnsureCreated();
        return context;
    }
}
