namespace Gdc.Infrastructure.Auth;

public sealed record UserSummaryDto(Guid Id, Guid TenantId, string Email, string Status);

public interface IUserReadService
{
    Task<IReadOnlyList<UserSummaryDto>> ListCurrentTenantUsersAsync(CancellationToken cancellationToken = default);

    Task<UserSummaryDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSummaryDto>> ListAllUsersForSuperAdminAsync(CancellationToken cancellationToken = default);
}
