using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Auth;

public sealed class RbacMatrixService(GdcDbContext dbContext) : IRbacMatrixService
{
    public async Task<RbacMatrixResponse> GetMatrixAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await dbContext.Permissions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Code)
            .Select(p => new RbacPermissionDto(p.Id, p.Code, p.Module, p.Action, p.Description))
            .ToListAsync(cancellationToken);

        var roles = await dbContext.Roles
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Code)
            .Select(r => new { r.Id, r.Code, r.Name })
            .ToListAsync(cancellationToken);

        var assignments = await dbContext.RolePermissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var roleDtos = roles
            .Select(r => new RbacRoleMatrixDto(
                r.Id,
                r.Code,
                r.Name,
                assignments
                    .Where(a => a.RoleId == r.Id)
                    .Select(a => a.PermissionId)
                    .ToList()))
            .ToList();

        return new RbacMatrixResponse(permissions, roleDtos);
    }

    public async Task UpdateMatrixAsync(
        UpdateRbacMatrixRequest request,
        CancellationToken cancellationToken = default)
    {
        foreach (var update in request.Assignments)
        {
            var existing = await dbContext.RolePermissions
                .FirstOrDefaultAsync(
                    rp => rp.RoleId == update.RoleId && rp.PermissionId == update.PermissionId,
                    cancellationToken);

            if (update.Enabled)
            {
                if (existing is null)
                {
                    dbContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = update.RoleId,
                        PermissionId = update.PermissionId,
                    });
                }
            }
            else if (existing is not null)
            {
                dbContext.RolePermissions.Remove(existing);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
