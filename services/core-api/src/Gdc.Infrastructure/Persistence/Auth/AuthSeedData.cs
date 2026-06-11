using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Persistence.Auth;

internal static class AuthSeedData
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 6, 10, 0, 0, 0, TimeSpan.Zero);

    public static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = AuthRoleIds.SuperAdmin,
                Code = "SuperAdmin",
                Name = "Super Administrador",
                Description = "Acceso global de gobierno multi-tenant",
                IsActive = true,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            },
            new Role
            {
                Id = AuthRoleIds.TenantAdmin,
                Code = "TenantAdmin",
                Name = "Administrador de tenant",
                Description = "Administración de usuarios y permisos del tenant",
                IsActive = true,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            },
            new Role
            {
                Id = AuthRoleIds.Operator,
                Code = "Operator",
                Name = "Operador",
                Description = "Usuario operativo con permisos funcionales asignados",
                IsActive = true,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            });
    }

    public static void SeedPermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                Id = AuthPermissionIds.UsersRead,
                Code = "auth.users.read",
                Module = "auth",
                Action = "read",
                Description = "Consultar usuarios",
                IsActive = true,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            },
            new Permission
            {
                Id = AuthPermissionIds.UsersWrite,
                Code = "auth.users.write",
                Module = "auth",
                Action = "write",
                Description = "Gestionar usuarios",
                IsActive = true,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            },
            new Permission
            {
                Id = AuthPermissionIds.RbacManage,
                Code = "auth.rbac.manage",
                Module = "auth",
                Action = "manage",
                Description = "Administrar matriz RBAC",
                IsActive = true,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            });
    }

    public static void SeedDefaultRolePermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = AuthRoleIds.SuperAdmin, PermissionId = AuthPermissionIds.UsersRead },
            new RolePermission { RoleId = AuthRoleIds.SuperAdmin, PermissionId = AuthPermissionIds.UsersWrite },
            new RolePermission { RoleId = AuthRoleIds.SuperAdmin, PermissionId = AuthPermissionIds.RbacManage },
            new RolePermission { RoleId = AuthRoleIds.TenantAdmin, PermissionId = AuthPermissionIds.UsersRead },
            new RolePermission { RoleId = AuthRoleIds.TenantAdmin, PermissionId = AuthPermissionIds.UsersWrite },
            new RolePermission { RoleId = AuthRoleIds.Operator, PermissionId = AuthPermissionIds.UsersRead });
    }
}
