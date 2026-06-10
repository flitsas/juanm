import type { ReactNode } from "react";

type AuthShellProps = {
  children: ReactNode;
};

export function AuthShell({ children }: AuthShellProps) {
  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-[var(--flit-bg-app)]">
      <div
        className="absolute top-0 left-0 h-full w-1/2"
        style={{ background: "var(--flit-gradient-flit)" }}
        aria-hidden="true"
      >
        <div className="flex h-full flex-col items-center justify-center px-8 text-center">
          <div className="max-w-md">
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

      <div className="absolute top-0 right-0 flex h-full w-1/2 items-center justify-center bg-[var(--flit-bg-app)] px-8">
        <div className="w-full max-w-md rounded-[var(--flit-radius-card)] bg-white p-8 shadow-[var(--flit-shadow-card)]">
          {children}
        </div>
      </div>
    </div>
  );
}
