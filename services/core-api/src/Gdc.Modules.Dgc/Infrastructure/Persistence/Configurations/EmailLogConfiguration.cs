using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;

internal sealed class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("email_logs", "dgc");

        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.ComparendoId).HasColumnName("comparendo_id").IsRequired();
        builder.Property(e => e.SentAt).HasColumnName("sent_at").IsRequired();
        builder.Property(e => e.Origen).HasColumnName("origen").HasMaxLength(256).IsRequired();
        builder.Property(e => e.Destino).HasColumnName("destino").HasMaxLength(256).IsRequired();
        builder.Property(e => e.Cc).HasColumnName("cc").HasMaxLength(512);
        builder.Property(e => e.TipoAlerta).HasColumnName("tipo_alerta").HasMaxLength(64).IsRequired();
        builder.Property(e => e.EstadoEntrega).HasColumnName("estado_entrega").HasMaxLength(32).IsRequired();
        builder.Property(e => e.HtmlEvidencia).HasColumnName("html_evidencia");

        builder.HasOne(e => e.Comparendo)
            .WithMany(c => c.EmailLogs)
            .HasForeignKey(e => e.ComparendoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ComparendoId).HasDatabaseName("ix_email_logs_comparendo_id");
    }
}
