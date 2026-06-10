using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Persistence;

public sealed class GdcDbContext(DbContextOptions<GdcDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        base.OnModelCreating(modelBuilder);
    }
}
