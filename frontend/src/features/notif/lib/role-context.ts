export function getUserRole(): string {
  return process.env.NEXT_PUBLIC_USER_ROLE ?? "super_admin";
}

export function isSuperAdmin(): boolean {
  const role = getUserRole();
  return role === "super_admin" || role === "SuperAdmin";
}
