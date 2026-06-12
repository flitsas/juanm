using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Reglas.Infrastructure.Persistence.Configurations;

internal sealed class DynamicRuleConfiguration : IEntityTypeConfiguration<DynamicRule>
{
    public void Configure(EntityTypeBuilder<DynamicRule> builder)
    {
        builder.ToTable("dynamic_rules", "reglas");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(512);
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.PdfTemplateId).HasColumnName("pdf_template_id").IsRequired();
        builder.Property(e => e.EmailSubject).HasColumnName("email_subject").HasMaxLength(256).IsRequired();
        builder.Property(e => e.EmailBodyHtml).HasColumnName("email_body_html").IsRequired();
        builder.Property(e => e.SecretariatContactId).HasColumnName("secretariat_contact_id");

        builder.HasOne(e => e.SecretariatContact)
            .WithMany(c => c.DynamicRules)
            .HasForeignKey(e => e.SecretariatContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.TenantId, e.Name })
            .HasDatabaseName("uq_dynamic_rules_tenant_name")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");
    }
}
