using Gdc.Modules.Plantillas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Plantillas.Infrastructure.Persistence.Configurations;

internal sealed class DerechoPeticionConfiguration : IEntityTypeConfiguration<DerechoPeticion>
{
    public void Configure(EntityTypeBuilder<DerechoPeticion> builder)
    {
        builder.ToTable("derechos_peticion", "gdc");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(d => d.ComparendoId).HasColumnName("comparendo_id").IsRequired();
        builder.Property(d => d.PdfTemplateId).HasColumnName("pdf_template_id").IsRequired();
        builder.Property(d => d.TemplateVersion).HasColumnName("template_version").IsRequired();
        builder.Property(d => d.Estado).HasColumnName("estado").HasMaxLength(32).IsRequired();
        builder.Property(d => d.OutputStorageKey).HasColumnName("output_storage_key").HasMaxLength(512).IsRequired();
        builder.Property(d => d.GeneratedAt).HasColumnName("generated_at").IsRequired();

        builder.HasOne(d => d.PdfTemplate)
            .WithMany(t => t.DerechosPeticion)
            .HasForeignKey(d => d.PdfTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.ComparendoId).HasDatabaseName("ix_derechos_peticion_comparendo_id");
        builder.HasIndex(d => new { d.TenantId, d.ComparendoId, d.Estado })
            .HasDatabaseName("ix_derechos_peticion_tenant_comparendo_estado");
        builder.HasIndex(d => d.PdfTemplateId).HasDatabaseName("ix_derechos_peticion_pdf_template_id");
    }
}
