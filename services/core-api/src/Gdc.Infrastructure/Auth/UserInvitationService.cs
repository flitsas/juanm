using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Auth;

public sealed class UserInvitationService(
    GdcDbContext dbContext,
    ITenantContext tenantContext,
    IEmailSender emailSender,
    IOptions<InvitationSettings> invitationOptions) : IUserInvitationService
{
    private static readonly PasswordHasher<User> PasswordHasher = new();
    public async Task<(InviteUserResponse? Success, InvitationError? Error)> InviteAsync(
        InviteUserRequest request,
        CancellationToken cancellationToken = default)
    {
        tenantContext.EnableBypass();

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return (null, new InvitationError(InvitationErrorCode.DuplicateEmail, "Email inválido."));
        }

        var tenantExists = await dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Id == request.TenantId && t.IsActive, cancellationToken);

        if (!tenantExists)
        {
            return (null, new InvitationError(InvitationErrorCode.TenantNotFound, "Tenant no encontrado."));
        }

        var duplicate = await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(
                u => u.TenantId == request.TenantId && u.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (duplicate)
        {
            return (null, new InvitationError(
                InvitationErrorCode.DuplicateEmail,
                "El email ya está registrado en este tenant."));
        }

        var roleCode = string.IsNullOrWhiteSpace(request.RoleCode) ? AuthRoles.Operator : request.RoleCode.Trim();
        var roleId = ResolveRoleId(roleCode);
        if (roleId is null)
        {
            return (null, new InvitationError(InvitationErrorCode.InvalidRole, "Rol inválido."));
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Email = normalizedEmail,
            Status = UserStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        };
        user.PasswordHash = PasswordHasher.HashPassword(user, Guid.NewGuid().ToString("N"));

        var rawToken = TokenHasher.GenerateToken();
        var settings = invitationOptions.Value;
        var activationToken = new ActivationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAt = now.AddHours(settings.ActivationTokenHours),
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId.Value });
        dbContext.ActivationTokens.Add(activationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var activationUrl = $"{settings.ActivationBaseUrl.TrimEnd('/')}?token={rawToken}";
        await emailSender.SendAsync(
            new EmailMessage(
                normalizedEmail,
                "Activación de cuenta GDC",
                $"Utilice el siguiente enlace para activar su cuenta (válido {settings.ActivationTokenHours} horas): {activationUrl}"),
            cancellationToken);

        return (new InviteUserResponse(user.Id, user.TenantId, user.Email, user.Status.ToString()), null);
    }

    public async Task<(ActivateUserResponse? Success, InvitationError? Error)> ActivateAsync(
        ActivateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return (null, new InvitationError(InvitationErrorCode.InvalidToken, "Token de activación inválido."));
        }

        if (!PasswordPolicy.IsCompliant(request.Password))
        {
            return (null, new InvitationError(
                InvitationErrorCode.InvalidPassword,
                PasswordPolicy.InvalidMessage));
        }

        var tokenHash = TokenHasher.Hash(request.Token.Trim());
        var activation = await dbContext.ActivationTokens
            .IgnoreQueryFilters()
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.UsedAt == null,
                cancellationToken);

        if (activation is null)
        {
            return (null, new InvitationError(InvitationErrorCode.InvalidToken, "Token de activación inválido."));
        }

        if (activation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return (null, new InvitationError(
                InvitationErrorCode.ExpiredToken,
                "El token de activación ha expirado."));
        }

        var user = activation.User;
        if (user.Status != UserStatus.Pending)
        {
            return (null, new InvitationError(
                InvitationErrorCode.UserNotPending,
                "La cuenta no está pendiente de activación."));
        }

        var now = DateTimeOffset.UtcNow;
        user.PasswordHash = PasswordHasher.HashPassword(user, request.Password);
        user.Status = UserStatus.Active;
        user.UpdatedAt = now;
        activation.UsedAt = now;
        activation.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        return (new ActivateUserResponse(user.Id, user.Email, user.Status.ToString()), null);
    }

    private static Guid? ResolveRoleId(string roleCode) =>
        roleCode switch
        {
            AuthRoles.SuperAdmin => AuthRoleIds.SuperAdmin,
            AuthRoles.TenantAdmin => AuthRoleIds.TenantAdmin,
            AuthRoles.Operator => AuthRoleIds.Operator,
            _ => null,
        };

}
