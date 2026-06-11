using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Infrastructure.Persistence.Auth.Configurations;

internal sealed class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("revoked_tokens", "auth");

        AuditableEntityConfiguration.ConfigureAuditable(builder);

        builder.Property(t => t.TokenId)
            .HasColumnName("token_id")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz");

        builder.Property(t => t.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz");

        builder.HasIndex(t => t.TokenId)
            .IsUnique()
            .HasDatabaseName("uq_revoked_tokens_token_id")
            .HasFilter("deleted_at IS NULL");
    }
}
