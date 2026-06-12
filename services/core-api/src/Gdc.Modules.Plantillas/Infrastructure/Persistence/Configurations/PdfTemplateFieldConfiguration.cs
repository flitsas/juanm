using Gdc.Modules.Plantillas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Plantillas.Infrastructure.Persistence.Configurations;

internal sealed class PdfTemplateFieldConfiguration : IEntityTypeConfiguration<PdfTemplateField>
{
    public void Configure(EntityTypeBuilder<PdfTemplateField> builder)
    {
        builder.ToTable("pdf_template_fields", "gdc");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(f => f.PdfTemplateId).HasColumnName("pdf_template_id").IsRequired();
        builder.Property(f => f.AcroformName).HasColumnName("acroform_name").HasMaxLength(128).IsRequired();
        builder.Property(f => f.FieldType).HasColumnName("field_type").HasMaxLength(32).IsRequired();
        builder.Property(f => f.SystemVariable).HasColumnName("system_variable").HasMaxLength(128);
        builder.Property(f => f.ChoiceOptionsJson).HasColumnName("choice_options_json").HasColumnType("jsonb");
        builder.Property(f => f.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasOne(f => f.PdfTemplate)
            .WithMany(t => t.Fields)
            .HasForeignKey(f => f.PdfTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.PdfTemplateId, f.AcroformName })
            .HasDatabaseName("uq_pdf_template_fields_template_acroform")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(f => f.PdfTemplateId).HasDatabaseName("ix_pdf_template_fields_pdf_template_id");
    }
}
