using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Tests;

public static class AuthTestData
{
    public static readonly Guid TenantId = Guid.Parse("22222222-2222-4222-8222-222222222201");
    public static readonly Guid TenantBId = Guid.Parse("22222222-2222-4222-8222-222222222202");
    public static readonly Guid UserId = Guid.Parse("33333333-3333-4333-8333-333333333301");
    public static readonly Guid UserBId = Guid.Parse("33333333-3333-4333-8333-333333333302");
    public static readonly Guid SuperAdminUserId = Guid.Parse("33333333-3333-4333-8333-333333333303");
    public const string Email = "operator@example.com";
    public const string UserBEmail = "operator-b@example.com";
    public const string SuperAdminEmail = "superadmin@example.com";
    public const string Password = "Str0ng!Pass";

    public static async Task SeedActiveUserAsync(GdcDbContext context)
    {
        var now = DateTimeOffset.UtcNow;
        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Id = UserId,
            TenantId = TenantId,
            Email = Email,
            Status = UserStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
        };
        user.PasswordHash = hasher.HashPassword(user, Password);

        context.Tenants.Add(new Tenant
        {
            Id = TenantId,
            Name = "Tenant Test",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        if (!context.Roles.Any(r => r.Id == AuthRoleIds.Operator))
        {
            context.Roles.Add(new Role
            {
                Id = AuthRoleIds.Operator,
                Code = "Operator",
                Name = "Operador",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        context.Users.Add(user);
        context.UserRoles.Add(new UserRole { UserId = UserId, RoleId = AuthRoleIds.Operator });
        await context.SaveChangesAsync();
    }

    public static async Task SeedMultiTenantUsersAsync(GdcDbContext context)
    {
        await SeedActiveUserAsync(context);

        var now = DateTimeOffset.UtcNow;
        var hasher = new PasswordHasher<User>();

        context.Tenants.Add(new Tenant
        {
            Id = TenantBId,
            Name = "Tenant B",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        var userB = new User
        {
            Id = UserBId,
            TenantId = TenantBId,
            Email = UserBEmail,
            Status = UserStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
        };
        userB.PasswordHash = hasher.HashPassword(userB, Password);
        context.Users.Add(userB);
        context.UserRoles.Add(new UserRole { UserId = UserBId, RoleId = AuthRoleIds.Operator });

        if (!context.Roles.Any(r => r.Id == AuthRoleIds.SuperAdmin))
        {
            context.Roles.Add(new Role
            {
                Id = AuthRoleIds.SuperAdmin,
                Code = "SuperAdmin",
                Name = "Super Administrador",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        var superAdmin = new User
        {
            Id = SuperAdminUserId,
            TenantId = TenantId,
            Email = SuperAdminEmail,
            Status = UserStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
        };
        superAdmin.PasswordHash = hasher.HashPassword(superAdmin, Password);
        context.Users.Add(superAdmin);
        context.UserRoles.Add(new UserRole { UserId = SuperAdminUserId, RoleId = AuthRoleIds.SuperAdmin });

        await context.SaveChangesAsync();
    }
}
