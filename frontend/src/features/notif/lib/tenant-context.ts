import { getSession } from "@/features/auth/lib/session";

export function getTenantId(): string {
  const sessionTenant = getSession()?.tenantId;
  if (sessionTenant) return sessionTenant;
  return process.env.NEXT_PUBLIC_TENANT_ID ?? "22222222-2222-2222-2222-222222222222";
}
