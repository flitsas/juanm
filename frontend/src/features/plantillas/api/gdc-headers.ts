import { getSession } from "@/features/auth/lib/session";
import { getTenantId } from "../lib/tenant-context";

export function buildGdcHeaders(options?: { json?: boolean }): HeadersInit {
  const headers: Record<string, string> = {
    Accept: "application/json",
    "X-Tenant-Id": getTenantId(),
  };

  if (options?.json) {
    headers["Content-Type"] = "application/json";
  }

  const token = getSession()?.accessToken;
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}
