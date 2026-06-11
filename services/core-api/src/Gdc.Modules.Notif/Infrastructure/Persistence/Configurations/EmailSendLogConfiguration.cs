using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Notif.Infrastructure.Persistence.Configurations;

internal sealed class EmailSendLogConfiguration : IEntityTypeConfiguration<EmailSendLog>
{
    public void Configure(EntityTypeBuilder<EmailSendLog> builder)
    {
        builder.ToTable("email_send_logs", "notif");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.EmailQueueId).HasColumnName("email_queue_id");
        builder.Property(e => e.ComparendoId).HasColumnName("comparendo_id").IsRequired();
        builder.Property(e => e.Destino).HasColumnName("destino").HasMaxLength(256).IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(e => e.SentAt).HasColumnName("sent_at");
        builder.Property(e => e.ProviderMessageId).HasColumnName("provider_message_id").HasMaxLength(128);
        builder.Property(e => e.DgcEmailLogId).HasColumnName("dgc_email_log_id");

        builder.HasOne(e => e.EmailQueue)
            .WithMany(q => q.SendLogs)
            .HasForeignKey(e => e.EmailQueueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.ComparendoId).HasDatabaseName("ix_email_send_logs_comparendo_id");
        builder.HasIndex(e => e.EmailQueueId).HasDatabaseName("ix_email_send_logs_email_queue_id");
    }
}
