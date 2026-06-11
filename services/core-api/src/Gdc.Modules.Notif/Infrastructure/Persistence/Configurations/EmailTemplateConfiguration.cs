using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Notif.Infrastructure.Persistence.Configurations;

internal sealed class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("email_templates", "notif");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(256).IsRequired();
        builder.Property(e => e.HtmlBody).HasColumnName("html_body").IsRequired();
        builder.Property(e => e.BannerUrl).HasColumnName("banner_url").HasMaxLength(512);
        builder.Property(e => e.FooterUrl).HasColumnName("footer_url").HasMaxLength(512);

        builder.HasIndex(e => new { e.TenantId, e.Name })
            .HasDatabaseName("uq_email_templates_tenant_name")
            .IsUnique()
            .HasFilter("deleted_at IS NULL");
    }
}
