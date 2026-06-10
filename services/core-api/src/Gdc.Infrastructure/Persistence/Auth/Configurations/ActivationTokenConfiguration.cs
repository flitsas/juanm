using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Infrastructure.Persistence.Auth.Configurations;

internal sealed class ActivationTokenConfiguration : IEntityTypeConfiguration<ActivationToken>
{
    public void Configure(EntityTypeBuilder<ActivationToken> builder)
    {
        builder.ToTable("activation_tokens", "auth");

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
            .HasDatabaseName("uq_activation_tokens_token_hash")
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_activation_tokens_user_id");

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .HasConstraintName("fk_activation_tokens_users")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
