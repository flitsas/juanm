using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Infrastructure.Persistence.Auth.Configurations;

internal sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens", "auth");

        AuditableEntityConfiguration.ConfigureAuditable(builder);

        builder.Property(t => t.UserId).HasColumnName("user_id");

        builder.Property(t => t.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.Property(t => t.UsedAt)
            .HasColumnName("used_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("uq_password_reset_tokens_token_hash")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_password_reset_tokens_user_id");

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .HasConstraintName("fk_password_reset_tokens_users")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
