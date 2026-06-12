using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Reglas.Infrastructure.Persistence.Configurations;

internal sealed class SecretariatContactConfiguration : IEntityTypeConfiguration<SecretariatContact>
{
    public void Configure(EntityTypeBuilder<SecretariatContact> builder)
    {
        builder.ToTable("secretariat_contacts", "reglas");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.SecretariatCode).HasColumnName("secretariat_code").HasMaxLength(32).IsRequired();
        builder.Property(e => e.SecretariatName).HasColumnName("secretariat_name").HasMaxLength(128).IsRequired();
        builder.Property(e => e.ContactName).HasColumnName("contact_name").HasMaxLength(128).IsRequired();
        builder.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(256).IsRequired();
        builder.Property(e => e.ContactPhone).HasColumnName("contact_phone").HasMaxLength(32);
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.SecretariatCode })
            .HasDatabaseName("uq_secretariat_contacts_tenant_code")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");
    }
}
