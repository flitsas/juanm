using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Infrastructure.Persistence.Auth.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions", "auth");

        AuditableEntityConfiguration.ConfigureAuditable(builder);

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(p => p.Module)
            .HasColumnName("module")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.Action)
            .HasColumnName("action")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("uq_permissions_code")
            .HasFilter("deleted_at IS NULL");
    }
}
