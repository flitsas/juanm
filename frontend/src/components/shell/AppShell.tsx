"use client";

import { AmbientBackground } from "./AmbientBackground";
import { AppHeader } from "./AppHeader";
import { RightDock } from "./RightDock";

type AppShellProps = {
  children: React.ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  return (
    <div className="flex h-screen w-screen flex-col overflow-hidden bg-[var(--background)]">
      <AmbientBackground />
      <AppHeader />
      <main
        className="flex-1 overflow-y-auto pr-20"
        data-vertical-scroll
        data-testid="app-shell-main"
      >
        {children}
      </main>
      <RightDock />
    </div>
  );
}
