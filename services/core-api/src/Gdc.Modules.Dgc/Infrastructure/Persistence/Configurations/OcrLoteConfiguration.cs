using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Dgc.Infrastructure.Persistence.Configurations;

internal sealed class OcrLoteConfiguration : IEntityTypeConfiguration<OcrLote>
{
    public void Configure(EntityTypeBuilder<OcrLote> builder)
    {
        builder.ToTable("ocr_lotes", "dgc");

        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(o => o.Estado).HasColumnName("estado").HasMaxLength(32).IsRequired();
    }
}
