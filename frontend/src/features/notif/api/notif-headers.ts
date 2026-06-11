import { getUserRole } from "../lib/role-context";
import { getTenantId } from "../lib/tenant-context";

export function buildNotifHeaders(options?: { superAdmin?: boolean }): HeadersInit {
  const headers: Record<string, string> = {
    Accept: "application/json",
    "X-Tenant-Id": getTenantId(),
  };

  if (options?.superAdmin) {
    headers["X-Role"] = getUserRole();
  }

  return headers;
}
