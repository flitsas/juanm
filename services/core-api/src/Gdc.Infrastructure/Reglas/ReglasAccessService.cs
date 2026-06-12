using Gdc.Infrastructure.Auth;
using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasAccessService(IUserRoleContext roleContext)
{
    public void EnsureCanRead()
    {
        if (!HasAnyRole(AuthRoles.SuperAdmin, AuthRoles.TenantAdmin, AuthRoles.Operator))
        {
            throw new UnauthorizedAccessException("REGLAS read access required.");
        }
    }

    public void EnsureCanManage()
    {
        if (!HasAnyRole(AuthRoles.SuperAdmin, AuthRoles.TenantAdmin))
        {
            throw new UnauthorizedAccessException("REGLAS manage access required.");
        }
    }

    public void EnsureCanExecute()
    {
        if (!HasAnyRole(AuthRoles.SuperAdmin, AuthRoles.TenantAdmin))
        {
            throw new UnauthorizedAccessException("REGLAS execution access required.");
        }
    }

    private bool HasAnyRole(params string[] roles)
    {
        if (roleContext.IsSuperAdmin)
        {
            return true;
        }

        var code = roleContext.RoleCode;
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return roles.Any(role => string.Equals(code, role, StringComparison.OrdinalIgnoreCase));
    }
}
