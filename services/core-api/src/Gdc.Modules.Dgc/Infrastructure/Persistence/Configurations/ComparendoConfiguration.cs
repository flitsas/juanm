using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;

internal sealed class ComparendoConfiguration : IEntityTypeConfiguration<Comparendo>
{
    public void Configure(EntityTypeBuilder<Comparendo> builder)
    {
        builder.ToTable("comparendos", "dgc");

        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(c => c.NumeroComparendo).HasColumnName("numero_comparendo").HasMaxLength(64).IsRequired();
        builder.Property(c => c.Estado).HasColumnName("estado").HasMaxLength(32).IsRequired();
        builder.Property(c => c.InfractorNombre).HasColumnName("infractor_nombre").HasMaxLength(256);
        builder.Property(c => c.Documento).HasColumnName("documento").HasMaxLength(32);
        builder.Property(c => c.Placa).HasColumnName("placa").HasMaxLength(16);
        builder.Property(c => c.InfraccionCodigo).HasColumnName("infraccion_codigo").HasMaxLength(32);
        builder.Property(c => c.FechaComparendo).HasColumnName("fecha_comparendo");
        builder.Property(c => c.FechaNotificacion).HasColumnName("fecha_notificacion");
        builder.Property(c => c.SecretariaId).HasColumnName("secretaria_id");
        builder.Property(c => c.TotalValor).HasColumnName("total_valor").HasColumnType("numeric(15,2)");
        builder.Property(c => c.EstadoPago).HasColumnName("estado_pago").HasMaxLength(32);
        builder.Property(c => c.DpReferencia).HasColumnName("dp_referencia").HasMaxLength(128);
        builder.Property(c => c.Fuente).HasColumnName("fuente").HasMaxLength(16).IsRequired();
        builder.Property(c => c.PendienteContraventor).HasColumnName("pendiente_contraventor").IsRequired();
        builder.Property(c => c.UltimoIntentoAsociacion).HasColumnName("ultimo_intento_asociacion");

        builder.HasIndex(c => new { c.TenantId, c.NumeroComparendo })
            .IsUnique()
            .HasDatabaseName("uq_comparendos_tenant_numero");

        builder.HasOne(c => c.Contraventor)
            .WithOne(c => c.Comparendo)
            .HasForeignKey<Contraventor>(c => c.ComparendoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
