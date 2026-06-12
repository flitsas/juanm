"use client";

import { getSchedulerDisplayConfig } from "../lib/execution-labels";

export function SchedulerPanel() {
  const config = getSchedulerDisplayConfig();

  return (
    <div
      className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 shadow-sm"
      data-testid="reglas-scheduler-panel"
    >
      <h2 className="text-lg font-semibold text-[var(--deep)]">Motor programado</h2>
      <p className="mt-1 text-sm text-[var(--muted-foreground)]">
        El worker en segundo plano evalúa reglas activas y, si está habilitado, orquesta PDF y
        correo tras cada ciclo.
      </p>
      <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-3">
        <div>
          <dt className="text-xs text-[var(--muted-foreground)]">Estado</dt>
          <dd
            className={`font-medium ${config.enabled ? "text-[var(--tech)]" : "text-[var(--alert)]"}`}
            data-testid="reglas-scheduler-enabled"
          >
            {config.enabled ? "Activo" : "Desactivado"}
          </dd>
        </div>
        <div>
          <dt className="text-xs text-[var(--muted-foreground)]">Intervalo</dt>
          <dd className="font-medium text-[var(--deep)]" data-testid="reglas-scheduler-interval">
            cada {config.pollIntervalSeconds}s
          </dd>
        </div>
        <div>
          <dt className="text-xs text-[var(--muted-foreground)]">Post-evaluación</dt>
          <dd className="font-medium text-[var(--deep)]">
            {config.processMatchesAfterEvaluation ? "PDF + correo automático" : "Solo evaluación"}
          </dd>
        </div>
      </dl>
      <p className="mt-3 text-xs text-[var(--muted-foreground)]">
        Configuración servidor: <code className="font-mono">Reglas:Execution</code>. Valores
        mostrados vía variables <code className="font-mono">NEXT_PUBLIC_REGLAS_*</code> para
        referencia en UI.
      </p>
    </div>
  );
}
