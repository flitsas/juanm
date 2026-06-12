namespace Gdc.Modules.Notif.Application.Abstractions;

public interface IUserRoleContext
{
    bool IsSuperAdmin { get; }

    string? RoleCode { get; }
}
