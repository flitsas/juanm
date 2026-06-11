using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Notif.Infrastructure.Persistence.Configurations;

internal sealed class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
{
    public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.ToTable("email_queues", "notif");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.NotificationRuleId).HasColumnName("notification_rule_id").IsRequired();
        builder.Property(e => e.EmailTemplateId).HasColumnName("email_template_id").IsRequired();
        builder.Property(e => e.ComparendoId).HasColumnName("comparendo_id").IsRequired();
        builder.Property(e => e.Destino).HasColumnName("destino").HasMaxLength(256).IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(e => e.ScheduledAt).HasColumnName("scheduled_at").IsRequired();
        builder.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        builder.Property(e => e.ErrorMessage).HasColumnName("error_message");

        builder.HasOne(e => e.NotificationRule)
            .WithMany(r => r.QueueItems)
            .HasForeignKey(e => e.NotificationRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EmailTemplate)
            .WithMany()
            .HasForeignKey(e => e.EmailTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ComparendoId).HasDatabaseName("ix_email_queues_comparendo_id");
        builder.HasIndex(e => new { e.TenantId, e.Status, e.ScheduledAt })
            .HasDatabaseName("ix_email_queues_tenant_status_scheduled");
    }
}
