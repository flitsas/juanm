using Gdc.Infrastructure.Auth.Models;

namespace Gdc.Infrastructure.Auth;

public interface IPasswordRecoveryService
{
    Task<(ForgotPasswordResponse? Success, PasswordRecoveryError? Error)> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<(ResetPasswordResponse? Success, PasswordRecoveryError? Error)> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<(AdminSetPasswordResponse? Success, PasswordRecoveryError? Error)> AdminSetPasswordAsync(
        Guid userId,
        AdminSetPasswordRequest request,
        CancellationToken cancellationToken = default);
}
