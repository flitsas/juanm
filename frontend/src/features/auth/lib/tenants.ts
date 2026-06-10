import type { TenantGroup, UserSummary } from "../types";

export function groupUsersByTenant(users: UserSummary[]): TenantGroup[] {
  const map = new Map<string, UserSummary[]>();

  for (const user of users) {
    const list = map.get(user.tenantId) ?? [];
    list.push(user);
    map.set(user.tenantId, list);
  }

  return Array.from(map.entries()).map(([tenantId, tenantUsers]) => ({
    tenantId,
    label: `Tenant ${tenantId.slice(0, 8)}…`,
    users: tenantUsers,
  }));
}
