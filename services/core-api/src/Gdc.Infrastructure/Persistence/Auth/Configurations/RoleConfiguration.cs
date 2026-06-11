using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Infrastructure.Persistence.Auth.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", "auth");

        AuditableEntityConfiguration.ConfigureAuditable(builder);

        builder.Property(r => r.Code)
            .HasColumnName("code")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasIndex(r => r.Code)
            .IsUnique()
            .HasDatabaseName("uq_roles_code")
            .HasFilter("deleted_at IS NULL");
    }
}
