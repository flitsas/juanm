using Gdc.Modules.Plantillas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Plantillas.Infrastructure.Persistence.Configurations;

internal sealed class PdfTemplateConfiguration : IEntityTypeConfiguration<PdfTemplate>
{
    public void Configure(EntityTypeBuilder<PdfTemplate> builder)
    {
        builder.ToTable("pdf_templates", "gdc");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(t => t.StorageKey).HasColumnName("storage_key").HasMaxLength(512).IsRequired();
        builder.Property(t => t.Version).HasColumnName("version").IsRequired();
        builder.Property(t => t.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(500);

        builder.HasIndex(t => new { t.TenantId, t.Name })
            .HasDatabaseName("uq_pdf_templates_tenant_name")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");
    }
}
