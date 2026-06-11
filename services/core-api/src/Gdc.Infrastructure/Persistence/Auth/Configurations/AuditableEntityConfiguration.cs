using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Infrastructure.Persistence.Auth.Configurations;

internal static class AuditableEntityConfiguration
{
    public static void ConfigureAuditable<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");

        builder.Property(e => e.DeletedBy).HasColumnName("deleted_by");

        builder.Property(e => e.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("xid")
            .HasDefaultValueSql("'0'::xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}
