"use client";

import { Bell } from "lucide-react";
import { FlitLogoBlue } from "./FlitLogo";

/** Header sticky — flitready-suite/src/components/app-header.tsx */
export function AppHeader() {
  return (
    <header
      className="sticky top-0 z-30 flex h-16 items-center gap-4 border-b-2 bg-[var(--background)]/85 px-6 backdrop-blur-md"
      style={{ borderBottomColor: "var(--lime)" }}
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
          style={{ background: "#FF7A00" }}
        >
          3
        </span>
      </button>

      <div className="hidden items-center gap-2 border-l border-[var(--border)] pl-3 sm:flex">
        <div className="grid h-9 w-9 place-items-center rounded-full bg-gradient-flit text-sm font-bold text-white">
          WL
        </div>
        <div className="hidden flex-col leading-tight lg:flex">
          <span className="text-sm font-medium text-[var(--deep)]">Operador FLIT</span>
          <span className="text-[11px] font-light text-[var(--muted-foreground)]">Tenant dev</span>
        </div>
      </div>
    </header>
  );
}
