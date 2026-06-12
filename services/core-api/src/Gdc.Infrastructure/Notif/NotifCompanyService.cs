using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.TenantAdmin;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Notif;

public sealed class NotifCompanyService(
    GdcDbContext db,
    DgcTenantContext tenantContext,
    IUserRoleContext roleContext,
    TimeProvider timeProvider)
{
    public async Task<CompanyListResponse> ListCompaniesAsync(CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();

        var items = await db.TenantCompanies
            .IgnoreQueryFilters()
            .Where(c => c.DeletedAt == null)
            .OrderBy(c => c.Name)
            .Select(c => ToResponse(c))
            .ToListAsync(cancellationToken);

        return new CompanyListResponse(items);
    }

    public async Task<CompanyResponse?> GetCompanyAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();

        var company = await db.TenantCompanies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);

        return company is null ? null : ToResponse(company);
    }

    public async Task<CompanyResponse> CreateCompanyAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        ValidateCompanyName(request.Name);

        var now = timeProvider.GetUtcNow();
        var company = new TenantCompany
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            Nit = NormalizeOptional(request.Nit),
            ContactPhone = NormalizeOptional(request.ContactPhone),
            ContactEmail = NormalizeOptional(request.ContactEmail),
            IsActive = true,
            CreatedAt = now,
        };

        db.TenantCompanies.Add(company);
        await CoreTenantProvisioner.EnsureAsync(
            db,
            company.Id,
            company.Name,
            company.IsActive,
            company.CreatedAt,
            company.UpdatedAt,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(company);
    }

    public async Task<CompanyResponse?> UpdateCompanyAsync(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        ValidateCompanyName(request.Name);

        var company = await db.TenantCompanies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);

        if (company is null)
        {
            return null;
        }

        company.Name = request.Name.Trim();
        company.Nit = NormalizeOptional(request.Nit);
        company.ContactPhone = NormalizeOptional(request.ContactPhone);
        company.ContactEmail = NormalizeOptional(request.ContactEmail);
        if (request.IsActive.HasValue)
        {
            company.IsActive = request.IsActive.Value;
        }

        company.UpdatedAt = timeProvider.GetUtcNow();
        await CoreTenantProvisioner.EnsureAsync(
            db,
            company.Id,
            company.Name,
            company.IsActive,
            company.CreatedAt,
            company.UpdatedAt,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(company);
    }

    public async Task<bool> DeleteCompanyAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();

        var company = await db.TenantCompanies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, cancellationToken);

        if (company is null)
        {
            return false;
        }

        company.DeletedAt = timeProvider.GetUtcNow();
        company.IsActive = false;
        company.UpdatedAt = company.DeletedAt;
        await CoreTenantProvisioner.EnsureAsync(
            db,
            company.Id,
            company.Name,
            isActive: false,
            company.CreatedAt,
            company.UpdatedAt,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TenantProfileResponse?> GetProfileAsync(CancellationToken cancellationToken)
    {
        var company = await db.TenantCompanies
            .FirstOrDefaultAsync(c => c.Id == tenantContext.TenantId, cancellationToken);

        return company is null
            ? null
            : new TenantProfileResponse(
                company.Id,
                company.Name,
                company.ContactPhone,
                company.ContactEmail);
    }

    public async Task<TenantProfileResponse?> UpdateProfileAsync(
        UpdateTenantProfileRequest request,
        CancellationToken cancellationToken)
    {
        var company = await db.TenantCompanies
            .FirstOrDefaultAsync(c => c.Id == tenantContext.TenantId, cancellationToken);

        if (company is null)
        {
            return null;
        }

        if (request.ContactPhone is not null)
        {
            company.ContactPhone = NormalizeOptional(request.ContactPhone);
        }

        if (request.ContactEmail is not null)
        {
            company.ContactEmail = NormalizeOptional(request.ContactEmail);
        }

        company.UpdatedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);

        return new TenantProfileResponse(
            company.Id,
            company.Name,
            company.ContactPhone,
            company.ContactEmail);
    }

    public void EnsureSuperAdmin()
    {
        if (!roleContext.IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Super Admin role required.");
        }
    }

    private static CompanyResponse ToResponse(TenantCompany company) =>
        new(
            company.Id,
            company.Name,
            company.Nit,
            company.ContactPhone,
            company.ContactEmail,
            company.IsActive,
            company.CreatedAt,
            company.UpdatedAt);

    private static void ValidateCompanyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Company name is required.", nameof(name));
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
