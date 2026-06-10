using Gdc.Infrastructure.Auth.Models;

namespace Gdc.Infrastructure.Auth;

public interface IUserInvitationService
{
    Task<(InviteUserResponse? Success, InvitationError? Error)> InviteAsync(
        InviteUserRequest request,
        CancellationToken cancellationToken = default);

    Task<(ActivateUserResponse? Success, InvitationError? Error)> ActivateAsync(
        ActivateUserRequest request,
        CancellationToken cancellationToken = default);
}
