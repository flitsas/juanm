using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Notif.Infrastructure.Persistence.Configurations;

internal sealed class NotificationRuleConfiguration : IEntityTypeConfiguration<NotificationRule>
{
    public void Configure(EntityTypeBuilder<NotificationRule> builder)
    {
        builder.ToTable("notification_rules", "notif");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.EmailTemplateId).HasColumnName("email_template_id").IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(e => e.TriggerType).HasColumnName("trigger_type").HasMaxLength(32).IsRequired();
        builder.Property(e => e.TriggerDays).HasColumnName("trigger_days");
        builder.Property(e => e.TriggerReference).HasColumnName("trigger_reference").HasMaxLength(32);
        builder.Property(e => e.TriggerEstado).HasColumnName("trigger_estado").HasMaxLength(32);
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasOne(e => e.EmailTemplate)
            .WithMany(t => t.Rules)
            .HasForeignKey(e => e.EmailTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EmailTemplateId).HasDatabaseName("ix_notification_rules_email_template_id");
    }
}
