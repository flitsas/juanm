namespace Gdc.Infrastructure.Auth.Models;

public sealed record InviteUserRequest(string Email, Guid TenantId, string? RoleCode = null);

public sealed record InviteUserResponse(Guid UserId, Guid TenantId, string Email, string Status);

public sealed record ActivateUserRequest(string Token, string Password);

public sealed record ActivateUserResponse(Guid UserId, string Email, string Status);

public enum InvitationErrorCode
{
    TenantNotFound,
    DuplicateEmail,
    InvalidRole,
    InvalidToken,
    ExpiredToken,
    InvalidPassword,
    UserNotPending,
    EmailDeliveryFailed,
}

public sealed record InvitationError(InvitationErrorCode Code, string Message);
