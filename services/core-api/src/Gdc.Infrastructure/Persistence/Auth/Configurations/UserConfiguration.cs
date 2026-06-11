using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Infrastructure.Persistence.Auth.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        AuditableEntityConfiguration.ConfigureAuditable(builder);

        builder.Property(u => u.TenantId).HasColumnName("tenant_id");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(u => u.FailedLoginCount)
            .HasColumnName("failed_login_count")
            .HasDefaultValue(0);

        builder.Property(u => u.LockedUntil)
            .HasColumnName("locked_until")
            .HasColumnType("timestamptz");

        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasDatabaseName("uq_users_tenant_email")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("ix_users_tenant_id");

        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .HasConstraintName("fk_users_tenants")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
