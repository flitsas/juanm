import { getSession } from "@/features/auth/lib/session";
import { getUserRole } from "../lib/role-context";
import { getTenantId } from "../lib/tenant-context";

export function buildNotifHeaders(options?: { superAdmin?: boolean }): HeadersInit {
  const headers: Record<string, string> = {
    Accept: "application/json",
    "X-Tenant-Id": getTenantId(),
  };

  const token = getSession()?.accessToken;
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  if (options?.superAdmin) {
    headers["X-Role"] = getUserRole();
  }

  return headers;
}
