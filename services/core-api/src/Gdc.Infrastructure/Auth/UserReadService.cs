using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Auth;

public sealed class UserReadService(GdcDbContext dbContext, ITenantContext tenantContext) : IUserReadService
{
    public async Task<IReadOnlyList<UserSummaryDto>> ListCurrentTenantUsersAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new UserSummaryDto(u.Id, u.TenantId, u.Email, u.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSummaryDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserSummaryDto(u.Id, u.TenantId, u.Email, u.Status.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserSummaryDto>> ListAllUsersForSuperAdminAsync(
        CancellationToken cancellationToken = default)
    {
        tenantContext.EnableBypass();

        return await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(u => u.TenantId)
            .ThenBy(u => u.Email)
            .Select(u => new UserSummaryDto(u.Id, u.TenantId, u.Email, u.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}
