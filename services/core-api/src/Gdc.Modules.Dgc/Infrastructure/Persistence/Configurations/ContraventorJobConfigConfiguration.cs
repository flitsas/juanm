using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;

internal sealed class ContraventorJobConfigConfiguration : IEntityTypeConfiguration<ContraventorJobConfig>
{
    public void Configure(EntityTypeBuilder<ContraventorJobConfig> builder)
    {
        builder.ToTable("contraventor_job_configs", "dgc");

        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(c => c.CronExpression).HasColumnName("cron_expression").HasMaxLength(64).IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(c => c.WindowOrder).HasColumnName("window_order").IsRequired();

        builder.HasIndex(c => new { c.TenantId, c.WindowOrder })
            .IsUnique()
            .HasDatabaseName("uq_contraventor_job_configs_tenant_window");
    }
}
