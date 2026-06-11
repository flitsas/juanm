using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;

internal sealed class DescuentoMatrizConfiguration : IEntityTypeConfiguration<DescuentoMatriz>
{
    public void Configure(EntityTypeBuilder<DescuentoMatriz> builder)
    {
        builder.ToTable("descuento_matrices", "dgc");

        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(d => d.SecretariaId).HasColumnName("secretaria_id");
        builder.Property(d => d.DiasDescuento).HasColumnName("dias_descuento").IsRequired();
        builder.Property(d => d.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasIndex(d => new { d.TenantId, d.SecretariaId })
            .IsUnique()
            .HasDatabaseName("uq_descuento_matrices_tenant_secretaria");
    }
}
