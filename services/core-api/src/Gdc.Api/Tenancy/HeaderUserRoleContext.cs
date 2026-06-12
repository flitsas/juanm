using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Api.Tenancy;

public sealed class HeaderUserRoleContext(IHttpContextAccessor httpContextAccessor) : IUserRoleContext
{
    public string? RoleCode =>
        httpContextAccessor.HttpContext?.Request.Headers["X-Role"].FirstOrDefault();

    public bool IsSuperAdmin =>
        string.Equals(RoleCode, "super_admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(RoleCode, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
}
