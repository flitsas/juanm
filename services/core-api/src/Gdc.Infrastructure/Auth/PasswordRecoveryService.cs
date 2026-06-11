using Gdc.Infrastructure.Auth.Models;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Persistence.Auth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Auth;

public sealed class PasswordRecoveryService(
    GdcDbContext dbContext,
    IEmailSender emailSender,
    IOptions<PasswordRecoverySettings> recoveryOptions) : IPasswordRecoveryService
{
    private static readonly PasswordHasher<User> PasswordHasher = new();

    public async Task<(ForgotPasswordResponse? Success, PasswordRecoveryError? Error)> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return (new ForgotPasswordResponse(true), null);
        }

        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Email.ToLower() == normalizedEmail && u.Status == UserStatus.Active,
                cancellationToken);

        if (user is null)
        {
            return (new ForgotPasswordResponse(true), null);
        }

        var now = DateTimeOffset.UtcNow;
        var rawToken = TokenHasher.GenerateToken();
        var settings = recoveryOptions.Value;

        dbContext.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAt = now.AddHours(settings.ResetTokenHours),
            CreatedAt = now,
            UpdatedAt = now,
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var resetUrl = $"{settings.ResetBaseUrl.TrimEnd('/')}?token={rawToken}";
        await emailSender.SendAsync(
            new EmailMessage(
                normalizedEmail,
                "Recuperación de contraseña GDC",
                $"Utilice el siguiente enlace para restablecer su contraseña (válido {settings.ResetTokenHours} horas): {resetUrl}"),
            cancellationToken);

        return (new ForgotPasswordResponse(true), null);
    }

    public async Task<(ResetPasswordResponse? Success, PasswordRecoveryError? Error)> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.InvalidToken,
                "Token de recuperación inválido."));
        }

        if (!PasswordPolicy.IsCompliant(request.Password))
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.InvalidPassword,
                PasswordPolicy.InvalidMessage));
        }

        var tokenHash = TokenHasher.Hash(request.Token.Trim());
        var resetToken = await dbContext.PasswordResetTokens
            .IgnoreQueryFilters()
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.UsedAt == null,
                cancellationToken);

        if (resetToken is null)
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.InvalidToken,
                "Token de recuperación inválido."));
        }

        if (resetToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.ExpiredToken,
                "El token de recuperación ha expirado."));
        }

        var user = resetToken.User;
        if (user.Status != UserStatus.Active)
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.UserNotActive,
                "La cuenta no está activa."));
        }

        var now = DateTimeOffset.UtcNow;
        user.PasswordHash = PasswordHasher.HashPassword(user, request.Password);
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.UpdatedAt = now;
        resetToken.UsedAt = now;
        resetToken.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        return (new ResetPasswordResponse(user.Id, user.Email), null);
    }

    public async Task<(AdminSetPasswordResponse? Success, PasswordRecoveryError? Error)> AdminSetPasswordAsync(
        Guid userId,
        AdminSetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!PasswordPolicy.IsCompliant(request.Password))
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.InvalidPassword,
                PasswordPolicy.InvalidMessage));
        }

        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.UserNotFound,
                "Usuario no encontrado."));
        }

        if (user.Status != UserStatus.Active)
        {
            return (null, new PasswordRecoveryError(
                PasswordRecoveryErrorCode.UserNotActive,
                "La cuenta no está activa."));
        }

        var now = DateTimeOffset.UtcNow;
        user.PasswordHash = PasswordHasher.HashPassword(user, request.Password);
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return (new AdminSetPasswordResponse(user.Id), null);
    }
}
