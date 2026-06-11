using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;

internal sealed class ContraventorConfiguration : IEntityTypeConfiguration<Contraventor>
{
    public void Configure(EntityTypeBuilder<Contraventor> builder)
    {
        builder.ToTable("contraventors", "dgc");

        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(c => c.ComparendoId).HasColumnName("comparendo_id").IsRequired();
        builder.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(256).IsRequired();
        builder.Property(c => c.Documento).HasColumnName("documento").HasMaxLength(32).IsRequired();
        builder.Property(c => c.Correo).HasColumnName("correo").HasMaxLength(256);
        builder.Property(c => c.AsociacionAutomatica).HasColumnName("asociacion_automatica").IsRequired();

        builder.HasIndex(c => c.ComparendoId)
            .IsUnique()
            .HasDatabaseName("uq_contraventors_comparendo_id");
    }
}
