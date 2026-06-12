using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Api.Tests.Reglas;

internal static class ReglasTestSupport
{
    internal static ReglasAccessService TenantAdminAccess() =>
        new(new FakeRoleContext(AuthRoles.TenantAdmin));

    internal static ReglasAccessService OperatorAccess() =>
        new(new FakeRoleContext(AuthRoles.Operator));

    internal sealed class FakeRoleContext(string roleCode) : IUserRoleContext
    {
        public string? RoleCode => roleCode;

        public bool IsSuperAdmin =>
            string.Equals(roleCode, AuthRoles.SuperAdmin, StringComparison.OrdinalIgnoreCase);
    }
}
