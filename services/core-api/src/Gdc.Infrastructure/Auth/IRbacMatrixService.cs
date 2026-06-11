using Gdc.Infrastructure.Auth.Models;

namespace Gdc.Infrastructure.Auth;

public interface IRbacMatrixService
{
    Task<RbacMatrixResponse> GetMatrixAsync(CancellationToken cancellationToken = default);

    Task UpdateMatrixAsync(UpdateRbacMatrixRequest request, CancellationToken cancellationToken = default);
}
