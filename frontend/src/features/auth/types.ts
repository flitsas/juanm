export type AuthSession = {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  tenantId: string;
  email: string;
  role: string;
};

export type LoginPayload = {
  email: string;
  password: string;
};

export type ActivateAccountPayload = {
  token: string;
  password: string;
};

export type ActivateAccountResult = {
  userId: string;
  email: string;
  status: string;
};

export type UserSummary = {
  id: string;
  tenantId: string;
  email: string;
  status: string;
};

export type InviteUserPayload = {
  email: string;
  tenantId: string;
  roleCode?: string;
};

export type InviteUserResult = {
  userId: string;
  tenantId: string;
  email: string;
  status: string;
};

export type RbacPermission = {
  id: string;
  code: string;
  module: string;
  action: string;
  description?: string | null;
};

export type RbacRoleMatrix = {
  roleId: string;
  code: string;
  name: string;
  permissionIds: string[];
};

export type RbacMatrix = {
  permissions: RbacPermission[];
  roles: RbacRoleMatrix[];
};

export type RbacAssignmentUpdate = {
  roleId: string;
  permissionId: string;
  enabled: boolean;
};

export type TenantGroup = {
  tenantId: string;
  label: string;
  users: UserSummary[];
};
