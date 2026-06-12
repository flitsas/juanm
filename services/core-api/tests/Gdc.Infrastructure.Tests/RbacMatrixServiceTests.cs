using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Tests;

/// <summary>
/// Uso de ejemplo: consultar y actualizar la matriz RBAC por rol.
/// </summary>
public class RbacMatrixServiceTests
{
    [Fact]
    public async Task GetMatrixAsync_returns_roles_and_permissions()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new RbacMatrixService(context);

        var matrix = await service.GetMatrixAsync();

        Assert.Equal(6, matrix.Permissions.Count);
        Assert.Equal(3, matrix.Roles.Count);
        Assert.Contains(matrix.Permissions, p => p.Code == "auth.users.read");
        Assert.Contains(matrix.Permissions, p => p.Code == "reglas.read");
        Assert.Contains(matrix.Roles, r => r.Code == "Operator");
    }

    [Fact]
    public async Task UpdateMatrixAsync_persists_assignment_changes()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new RbacMatrixService(context);

        await service.UpdateMatrixAsync(new UpdateRbacMatrixRequest([
            new RbacAssignmentUpdate(AuthRoleIds.Operator, AuthPermissionIds.UsersWrite, true),
        ]));

        var operatorRole = await context.RolePermissions
            .Where(rp => rp.RoleId == AuthRoleIds.Operator)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        Assert.Contains(AuthPermissionIds.UsersWrite, operatorRole);
    }

    [Fact]
    public async Task UpdateMatrixAsync_is_reflected_on_next_get()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new RbacMatrixService(context);

        await service.UpdateMatrixAsync(new UpdateRbacMatrixRequest([
            new RbacAssignmentUpdate(AuthRoleIds.Operator, AuthPermissionIds.UsersWrite, true),
        ]));

        var matrix = await service.GetMatrixAsync();
        var operatorRow = matrix.Roles.Single(r => r.Code == "Operator");

        Assert.Contains(AuthPermissionIds.UsersWrite, operatorRow.PermissionIds);
    }

    [Fact]
    public async Task UpdateMatrixAsync_can_remove_permission_from_role()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new RbacMatrixService(context);

        await service.UpdateMatrixAsync(new UpdateRbacMatrixRequest([
            new RbacAssignmentUpdate(AuthRoleIds.Operator, AuthPermissionIds.UsersRead, false),
        ]));

        var matrix = await service.GetMatrixAsync();
        var operatorRow = matrix.Roles.Single(r => r.Code == "Operator");

        Assert.DoesNotContain(AuthPermissionIds.UsersRead, operatorRow.PermissionIds);
    }
}
