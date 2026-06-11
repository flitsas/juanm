using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Dgc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Persistence;

public sealed class GdcDbContext(DbContextOptions<GdcDbContext> options) : DbContext(options)
{
    public DbSet<Comparendo> Comparendos => Set<Comparendo>();

    public DbSet<Contraventor> Contraventors => Set<Contraventor>();

    public DbSet<OcrLote> OcrLotes => Set<OcrLote>();

    public DbSet<OcrItem> OcrItems => Set<OcrItem>();

    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    public DbSet<ContraventorJobConfig> ContraventorJobConfigs => Set<ContraventorJobConfig>();

    public DbSet<DescuentoMatriz> DescuentoMatrices => Set<DescuentoMatriz>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyDgcConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}
