using System.Security.Claims;
using Gdc.Infrastructure.Auth.Models;

namespace Gdc.Infrastructure.Auth;

public interface IAuthSessionService
{
    Task<(LoginResponse? Success, AuthError? Error)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<(LoginResponse? Success, AuthError? Error)> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);

    Task<bool> IsTokenRevokedAsync(string tokenId, CancellationToken cancellationToken = default);
}
