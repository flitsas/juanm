"use client";

import { useEffect, useState } from "react";
import { getSession } from "@/features/auth/lib/session";
import type { AuthSession } from "@/features/auth/types";

export function getDisplayName(email: string): string {
  const local = email.split("@")[0] ?? email;
  return local
    .split(/[._-]+/)
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

export function getInitials(email: string): string {
  const name = getDisplayName(email);
  const parts = name.split(" ").filter(Boolean);
  if (parts.length >= 2) {
    return `${parts[0]?.[0] ?? ""}${parts[1]?.[0] ?? ""}`.toUpperCase();
  }
  return name.slice(0, 2).toUpperCase();
}

type UserSessionBadgeProps = {
  session: AuthSession | null;
};

/** Bloque usuario inline — flitready-suite app-header.tsx */
export function UserSessionBadge({ session }: UserSessionBadgeProps) {
  if (!session) return null;

  const displayName = getDisplayName(session.email);
  const initials = getInitials(session.email);

  return (
    <div
      className="hidden items-center gap-2 border-l border-[var(--border)] pl-3 sm:flex"
      data-testid="user-session-badge"
    >
      <div className="grid h-9 w-9 place-items-center rounded-full bg-gradient-flit text-sm font-bold text-white">
        {initials}
      </div>
      <div className="hidden flex-col leading-tight lg:flex">
        <span className="text-sm font-medium text-[var(--deep)]">{displayName}</span>
        <span className="text-[11px] font-light text-[var(--muted-foreground)] capitalize">
          {session.role}
        </span>
      </div>
    </div>
  );
}

export function useAuthSession(): AuthSession | null {
  const [session, setSession] = useState<AuthSession | null>(null);

  useEffect(() => {
    setSession(getSession());
    const onStorage = (event: StorageEvent) => {
      if (event.key === "gdc-auth-session") {
        setSession(getSession());
      }
    };
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  return session;
}
