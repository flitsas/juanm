using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Notif.Infrastructure.Persistence.Configurations;

internal sealed class TenantCompanyConfiguration : IEntityTypeConfiguration<TenantCompany>
{
    public void Configure(EntityTypeBuilder<TenantCompany> builder)
    {
        builder.ToTable("tenants", "identity");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Name).HasColumnName("name").IsRequired();
        builder.Property(t => t.Nit).HasColumnName("nit").HasMaxLength(32);
        builder.Property(t => t.ContactPhone).HasColumnName("contact_phone").HasMaxLength(32);
        builder.Property(t => t.ContactEmail).HasColumnName("contact_email").HasMaxLength(256);
        builder.Property(t => t.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        builder.Property(t => t.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}
