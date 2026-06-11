using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Api.Tenancy;

public sealed class HeaderUserRoleContext(IHttpContextAccessor httpContextAccessor) : IUserRoleContext
{
    public bool IsSuperAdmin
    {
        get
        {
            var role = httpContextAccessor.HttpContext?.Request.Headers["X-Role"].FirstOrDefault();
            return string.Equals(role, "super_admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
