using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Notif.Infrastructure.Persistence.Configurations;

internal sealed class EmailProviderConfigConfiguration : IEntityTypeConfiguration<EmailProviderConfig>
{
    public void Configure(EntityTypeBuilder<EmailProviderConfig> builder)
    {
        builder.ToTable("email_provider_configs", "notif");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.ProviderType).HasColumnName("provider_type").HasMaxLength(32).IsRequired();
        builder.Property(e => e.CredentialsEncrypted).HasColumnName("credentials_encrypted");
        builder.Property(e => e.FromAddress).HasColumnName("from_address").HasMaxLength(256).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.DispatchEnabled).HasColumnName("dispatch_enabled").IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.IsActive })
            .HasDatabaseName("ix_email_provider_configs_tenant_active")
            .IsUnique()
            .HasFilter("is_active = true AND deleted_at IS NULL");
    }
}
