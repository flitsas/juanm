"use client";

import type { ReactNode } from "react";
import { ShootingStars } from "@/components/flit/shooting-stars";
import { useLoginIntroPhase } from "../hooks/use-login-intro-phase";

const PANEL_EASING = "cubic-bezier(.65,.05,.36,1)";

type AuthShellProps = {
  children: ReactNode;
  /** Play the split-screen intro animation (login only). */
  intro?: boolean;
};

export function AuthShell({ children, intro = true }: AuthShellProps) {
  const phase = useLoginIntroPhase(intro);
  const collapsed = phase >= 2;
  const showForm = phase === 3;

  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-[var(--flit-bg-app)]">
      {/* Left gradient panel — animated width collapse */}
      <div
        className="absolute top-0 left-0 h-full transition-[width] duration-[800ms]"
        style={{
          width: collapsed ? "50%" : "100%",
          background: "var(--flit-gradient-flit)",
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
          <div className="flit-brand-pulse text-center">
            <p className="text-sm font-medium tracking-[0.2em] text-white/80 uppercase">
              Gestión Documental Corporativa
            </p>
            <h2 className="mt-4 text-4xl font-bold text-white">GDC 2.0</h2>
            <p className="mt-3 text-base font-light text-white/90">
              Plataforma integral de autenticación y administración multi-tenant.
            </p>
          </div>
        </div>
      </div>

      {/* Right form panel */}
      <div
        className="absolute top-0 right-0 flex h-full w-1/2 items-center justify-center bg-[var(--flit-bg-app)] px-8 transition-opacity duration-700"
        style={{
          opacity: showForm ? 1 : 0,
          pointerEvents: showForm ? "auto" : "none",
        }}
      >
        <div
          className="w-full max-w-md"
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
