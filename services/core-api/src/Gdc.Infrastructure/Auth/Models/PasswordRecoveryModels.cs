namespace Gdc.Infrastructure.Auth.Models;

public sealed record ForgotPasswordRequest(string Email);

public sealed record ForgotPasswordResponse(bool Accepted);

public sealed record ResetPasswordRequest(string Token, string Password);

public sealed record ResetPasswordResponse(Guid UserId, string Email);

public sealed record AdminSetPasswordRequest(string Password);

public sealed record AdminSetPasswordResponse(Guid UserId);

public enum PasswordRecoveryErrorCode
{
    InvalidToken,
    ExpiredToken,
    InvalidPassword,
    UserNotFound,
    UserNotActive,
}

public sealed record PasswordRecoveryError(PasswordRecoveryErrorCode Code, string Message);
