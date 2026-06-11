using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;

internal sealed class OcrItemConfiguration : IEntityTypeConfiguration<OcrItem>
{
    public void Configure(EntityTypeBuilder<OcrItem> builder)
    {
        builder.ToTable("ocr_items", "dgc");

        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(o => o.OcrLoteId).HasColumnName("ocr_lote_id").IsRequired();
        builder.Property(o => o.ComparendoId).HasColumnName("comparendo_id");
        builder.Property(o => o.ArchivoUri).HasColumnName("archivo_uri").HasMaxLength(1024).IsRequired();
        builder.Property(o => o.Estado).HasColumnName("estado").HasMaxLength(32).IsRequired();
        builder.Property(o => o.OcrPayloadJson).HasColumnName("ocr_payload_json").HasColumnType("jsonb");

        builder.HasOne(o => o.OcrLote)
            .WithMany(l => l.Items)
            .HasForeignKey(o => o.OcrLoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Comparendo)
            .WithMany(c => c.OcrItems)
            .HasForeignKey(o => o.ComparendoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.OcrLoteId).HasDatabaseName("ix_ocr_items_ocr_lote_id");
    }
}
