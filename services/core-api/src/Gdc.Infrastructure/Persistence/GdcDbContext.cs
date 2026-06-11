using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Dgc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Domain.Entities;
using Gdc.Modules.Notif.Infrastructure.Persistence;
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

    public DbSet<EmailProviderConfig> EmailProviderConfigs => Set<EmailProviderConfig>();

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();

    public DbSet<EmailQueue> EmailQueues => Set<EmailQueue>();

    public DbSet<EmailSendLog> EmailSendLogs => Set<EmailSendLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyDgcConfigurations();
        modelBuilder.ApplyNotifConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}
