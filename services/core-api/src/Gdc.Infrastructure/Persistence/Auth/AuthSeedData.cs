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
}
