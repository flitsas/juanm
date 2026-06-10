using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Tests;

public static class AuthTestData
{
    public static readonly Guid TenantId = Guid.Parse("22222222-2222-4222-8222-222222222201");
    public static readonly Guid UserId = Guid.Parse("33333333-3333-4333-8333-333333333301");
    public const string Email = "operator@example.com";
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

        context.Roles.Add(new Role
        {
            Id = AuthRoleIds.Operator,
            Code = "Operator",
            Name = "Operador",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        context.Users.Add(user);
        context.UserRoles.Add(new UserRole { UserId = UserId, RoleId = AuthRoleIds.Operator });
        await context.SaveChangesAsync();
    }

    public static GdcDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase($"auth-login-{Guid.NewGuid()}")
            .Options;

        return new GdcDbContext(options);
    }
}
