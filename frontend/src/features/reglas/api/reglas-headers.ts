import { getSession } from "@/features/auth/lib/session";
import { getUserRole } from "../lib/role-context";
import { getTenantId } from "../lib/tenant-context";

export function buildReglasHeaders(): HeadersInit {
  const headers: Record<string, string> = {
    Accept: "application/json",
    "X-Tenant-Id": getTenantId(),
    "X-Role": getUserRole(),
  };

  const token = getSession()?.accessToken;
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}
