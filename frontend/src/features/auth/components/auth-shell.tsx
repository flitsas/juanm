"use client";

import type { ReactNode } from "react";
import { ShootingStars } from "@/components/flit/shooting-stars";
import { FlitLogo } from "@/components/shell/FlitLogo";
import { ThemeToggle } from "@/components/shell/theme-toggle";
import { useLoginIntroPhase } from "../hooks/use-login-intro-phase";

const PANEL_EASING = "cubic-bezier(.65,.05,.36,1)";

type AuthShellProps = {
  children: ReactNode;
  /** Play the split-screen intro animation (login only). */
  intro?: boolean;
};

/** Split-screen login — flitready-suite/src/routes/login.tsx */
export function AuthShell({ children, intro = true }: AuthShellProps) {
  const phase = useLoginIntroPhase(intro);
  const collapsed = phase >= 2;
  const showForm = phase === 3;

  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-[var(--background)]">
      <div
        className="absolute top-0 left-0 h-full transition-[width] duration-[800ms]"
        style={{
          width: collapsed ? "50%" : "100%",
          background: "var(--gradient-flit)",
          transitionTimingFunction: PANEL_EASING,
        }}
        aria-hidden={showForm}
      >
        <ShootingStars count={10} tone="white" period={3} />
        <div
          className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 transition-all duration-[800ms]"
          style={{
            transitionTimingFunction: PANEL_EASING,
            width: collapsed ? "min(360px, 70%)" : "min(560px, 70%)",
          }}
        >
          <div className="flit-brand-pulse">
            <FlitLogo showTagline />
          </div>
        </div>
      </div>

      <div
        className="absolute top-0 right-0 flex h-full w-1/2 items-center justify-center bg-[var(--background)] px-8 transition-opacity duration-700"
        style={{
          opacity: showForm ? 1 : 0,
          pointerEvents: showForm ? "auto" : "none",
        }}
      >
        <ThemeToggle variant="icon" className="absolute top-6 right-6" />

        <div
          className="w-full max-w-sm"
          style={
            showForm
              ? { animation: "flit-slide-in-right 0.7s ease-out, flit-fade-in 0.7s ease-out" }
              : undefined
          }
        >
          {children}
        </div>
      </div>
    </div>
  );
}
