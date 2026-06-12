using Gdc.Infrastructure.Persistence.Auth;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Dgc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Domain.Entities;
using Gdc.Modules.Notif.Infrastructure.Persistence;
using Gdc.Modules.Plantillas.Domain.Entities;
using Gdc.Modules.Plantillas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Persistence;

public sealed class GdcDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public GdcDbContext(DbContextOptions<GdcDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ActivationToken> ActivationTokens => Set<ActivationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    public DbSet<Comparendo> Comparendos => Set<Comparendo>();

    public DbSet<Contraventor> Contraventors => Set<Contraventor>();

    public DbSet<OcrLote> OcrLotes => Set<OcrLote>();

    public DbSet<OcrItem> OcrItems => Set<OcrItem>();

    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    public DbSet<ContraventorJobConfig> ContraventorJobConfigs => Set<ContraventorJobConfig>();

    public DbSet<DescuentoMatriz> DescuentoMatrices => Set<DescuentoMatriz>();

    public DbSet<EmailProviderConfig> EmailProviderConfigs => Set<EmailProviderConfig>();

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();

    public DbSet<EmailQueue> EmailQueues => Set<EmailQueue>();

    public DbSet<EmailSendLog> EmailSendLogs => Set<EmailSendLog>();

    public DbSet<TenantCompany> TenantCompanies => Set<TenantCompany>();

    public DbSet<PdfTemplate> PdfTemplates => Set<PdfTemplate>();

    public DbSet<PdfTemplateField> PdfTemplateFields => Set<PdfTemplateField>();

    public DbSet<DerechoPeticion> DerechosPeticion => Set<DerechoPeticion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GdcDbContext).Assembly);
        AuthSeedData.SeedRoles(modelBuilder);
        AuthSeedData.SeedPermissions(modelBuilder);
        AuthSeedData.SeedDefaultRolePermissions(modelBuilder);
        modelBuilder.ApplyDgcConfigurations();
        modelBuilder.ApplyNotifConfigurations();
        modelBuilder.ApplyPlantillasConfigurations();

        modelBuilder.Entity<User>().HasQueryFilter(user =>
            user.DeletedAt == null &&
            (_tenantContext.BypassTenantFilter ||
             (_tenantContext.TenantId != null && user.TenantId == _tenantContext.TenantId)));

        base.OnModelCreating(modelBuilder);
    }
}
