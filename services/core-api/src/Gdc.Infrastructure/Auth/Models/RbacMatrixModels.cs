namespace Gdc.Infrastructure.Auth.Models;

public sealed record RbacPermissionDto(Guid Id, string Code, string Module, string Action, string? Description);

public sealed record RbacRoleMatrixDto(Guid RoleId, string Code, string Name, IReadOnlyList<Guid> PermissionIds);

public sealed record RbacMatrixResponse(
    IReadOnlyList<RbacPermissionDto> Permissions,
    IReadOnlyList<RbacRoleMatrixDto> Roles);

public sealed record RbacAssignmentUpdate(Guid RoleId, Guid PermissionId, bool Enabled);

public sealed record UpdateRbacMatrixRequest(IReadOnlyList<RbacAssignmentUpdate> Assignments);
