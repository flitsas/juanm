import { ensureAccessToken } from "@/features/auth/lib/ensure-access-token";
import { getUserRole } from "../lib/role-context";
import { getTenantId } from "../lib/tenant-context";

export async function buildNotifHeaders(options?: { superAdmin?: boolean }): Promise<HeadersInit> {
  const headers: Record<string, string> = {
    Accept: "application/json",
    "X-Tenant-Id": getTenantId(),
  };

  try {
    headers.Authorization = `Bearer ${await ensureAccessToken()}`;
  } catch {
    // Sin sesión válida: el API responderá 401.
  }

  if (options?.superAdmin) {
    headers["X-Role"] = getUserRole();
  }

  return headers;
}
