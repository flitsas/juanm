namespace Gdc.Infrastructure.Auth.Models;

public enum AuthErrorCode
{
    InvalidCredentials,
    AccountLocked,
    AccountNotActive,
}

public sealed record AuthError(AuthErrorCode Code, string Message);
