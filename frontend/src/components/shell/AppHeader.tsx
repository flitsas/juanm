"use client";

import { Bell, LogOut } from "lucide-react";
import { useLogout } from "@/features/auth/hooks/use-logout";
import { FlitLogoBlue } from "./FlitLogo";
import { ThemeToggle } from "./theme-toggle";
import { UserSessionBadge, useAuthSession } from "./user-menu";

/** Header sticky — flitready-suite app-header.tsx */
export function AppHeader() {
  const session = useAuthSession();
  const { onLogout, loading } = useLogout();

  return (
    <header
      className="sticky top-0 z-30 flex h-16 items-center gap-4 border-b-2 bg-[var(--background)]/85 px-6 backdrop-blur-md"
      style={{ borderBottomColor: "var(--flit-lime)" }}
    >
      <FlitLogoBlue href="/" />

      <div className="flex-1" />

      <button
        type="button"
        aria-label="Notificaciones"
        className="relative grid h-10 w-10 place-items-center rounded-full transition-colors hover:bg-[var(--muted)]"
      >
        <Bell className="size-5 text-[var(--deep)]" aria-hidden />
        <span
          className="absolute top-1 right-1 grid min-h-[18px] min-w-[18px] place-items-center rounded-full px-1 text-[10px] font-bold text-white ring-2 ring-[var(--background)]"
          style={{ background: "var(--flit-amber)" }}
        >
          3
        </span>
      </button>

      <ThemeToggle variant="pill" />

      <UserSessionBadge session={session} />

      <button
        type="button"
        aria-label="Cerrar sesión"
        disabled={loading}
        onClick={() => void onLogout()}
        className="grid h-10 w-10 place-items-center rounded-full text-[var(--deep)] transition-colors hover:bg-[var(--muted)] disabled:opacity-60"
        data-testid="header-logout"
      >
        <LogOut className="size-4" aria-hidden />
      </button>
    </header>
  );
}
