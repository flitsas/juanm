import type { AuthSession } from "../types";

export function isSuperAdmin(session: AuthSession | null): boolean {
  return session?.role === "SuperAdmin";
}
