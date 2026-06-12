using System.Diagnostics.CodeAnalysis;
using Gdc.Modules.Plantillas.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Plantillas.Infrastructure.Persistence.Configurations;

internal static class TenantAuditableEntityConfiguration
{
    public static void ConfigureTenantAuditable<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TEntity
    >(EntityTypeBuilder<TEntity> builder)
        where TEntity : TenantAuditableEntity
    {
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
        builder.Property(e => e.DeletedBy).HasColumnName("deleted_by");
        builder.Property(e => e.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("xid")
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasDefaultValue(0u)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
