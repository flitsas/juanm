import { getSession } from "@/features/auth/lib/session";

export function getUserRole(): string {
  const sessionRole = getSession()?.role;
  if (sessionRole) return sessionRole;
  return process.env.NEXT_PUBLIC_USER_ROLE ?? "TenantAdmin";
}
