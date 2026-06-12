using Gdc.Infrastructure.Notif;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.TenantAdmin;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Notif;

public sealed class NotifCompanyTests
{
    private static readonly Guid TenantA = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TenantB = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task CreateCompanyAsync_persists_for_super_admin()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db, TenantA, isSuperAdmin: true);

        var created = await service.CreateCompanyAsync(
            new CreateCompanyRequest("Acme Flota", "900123456", "+573001112233", "ops@acme.test"),
            CancellationToken.None);

        Assert.Equal("Acme Flota", created.Name);
        Assert.Equal("900123456", created.Nit);
        Assert.Single(await db.TenantCompanies.IgnoreQueryFilters().Where(c => c.Name == "Acme Flota").ToListAsync());
    }

    [Fact]
    public async Task ListCompaniesAsync_returns_global_roster_for_super_admin()
    {
        await using var db = CreateDbContext();
        SeedCompanies(db);
        var service = CreateService(db, TenantA, isSuperAdmin: true);

        var result = await service.ListCompaniesAsync(CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task DeleteCompanyAsync_throws_for_non_super_admin()
    {
        await using var db = CreateDbContext();
        var companyId = SeedCompanies(db).First();
        var service = CreateService(db, TenantA, isSuperAdmin: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.DeleteCompanyAsync(companyId, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateProfileAsync_updates_contact_fields_for_tenant()
    {
        await using var db = CreateDbContext();
        SeedCompanies(db);
        var service = CreateService(db, TenantA, isSuperAdmin: false);

        var updated = await service.UpdateProfileAsync(
            new UpdateTenantProfileRequest("+573009998877", "contacto@flit.dev"),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(TenantA, updated.TenantId);
        Assert.Equal("+573009998877", updated.ContactPhone);
        Assert.Equal("contacto@flit.dev", updated.ContactEmail);
    }

    [Fact]
    public async Task GetProfileAsync_returns_only_current_tenant()
    {
        await using var db = CreateDbContext();
        SeedCompanies(db);
        var service = CreateService(db, TenantB, isSuperAdmin: false);

        var profile = await service.GetProfileAsync(CancellationToken.None);

        Assert.NotNull(profile);
        Assert.Equal(TenantB, profile.TenantId);
        Assert.Equal("FLIT Test Tenant", profile.Name);
    }

    private static List<Guid> SeedCompanies(GdcDbContext db)
    {
        var companies = new List<TenantCompany>
        {
            new()
            {
                Id = TenantA,
                Name = "FLIT Dev Tenant",
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
            },
            new()
            {
                Id = TenantB,
                Name = "FLIT Test Tenant",
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
            },
        };

        db.TenantCompanies.AddRange(companies);
        db.SaveChanges();
        return companies.Select(c => c.Id).ToList();
    }

    private static NotifCompanyService CreateService(
        GdcDbContext db,
        Guid tenantId,
        bool isSuperAdmin) =>
        new(
            db,
            new FakeTenantContext(tenantId),
            new FakeRoleContext(isSuperAdmin),
            TimeProvider.System);

    private static GdcDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GdcDbContext(options, new TestSupport.TestTenantContext());
    }

    private sealed class FakeTenantContext(Guid tenantId) : Gdc.Modules.Dgc.Application.Abstractions.ITenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }

    private sealed class FakeRoleContext(bool isSuperAdmin) : IUserRoleContext
    {
        public bool IsSuperAdmin => isSuperAdmin;
    }
}
