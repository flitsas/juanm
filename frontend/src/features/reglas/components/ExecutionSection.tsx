"use client";

import { Button } from "primereact/button";
import { useEffect, useState } from "react";
import { getSession } from "@/features/auth/lib/session";
import { useReglasExecutionMutations, useReglasMatches, useReglasRuns } from "../api/use-runs";
import {
  canExecuteReglas,
  formatMatchStatus,
  formatRunMetrics,
  formatRunStatus,
  formatTriggerType,
} from "../lib/execution-labels";
import type { ReglasRun } from "../lib/reglas.types";
import { getUserRole } from "../lib/role-context";
import { SchedulerPanel } from "./SchedulerPanel";

function formatDateTime(value: string | null | undefined): string {
  if (!value) return "—";
  try {
    return new Intl.DateTimeFormat("es-CO", {
      dateStyle: "short",
      timeStyle: "short",
    }).format(new Date(value));
  } catch {
    return value;
  }
}

export function ExecutionSection() {
  const runsQuery = useReglasRuns();
  const { triggerRun, processRun } = useReglasExecutionMutations();
  const [selectedRunId, setSelectedRunId] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const runs = runsQuery.data?.items ?? [];
  const matchesQuery = useReglasMatches(selectedRunId, null, { enabled: Boolean(selectedRunId) });
  const matches = matchesQuery.data?.items ?? [];

  const role = getSession()?.role ?? getUserRole();
  const canExecute = canExecuteReglas(role);

  useEffect(() => {
    if (!selectedRunId && runs.length > 0) {
      setSelectedRunId(runs[0]!.id);
    }
  }, [runs, selectedRunId]);

  const handleManualRun = async () => {
    setNotice(null);
    try {
      const result = await triggerRun.mutateAsync();
      setSelectedRunId(result.runId);
      setNotice(
        `Evaluación manual iniciada. ${result.matchedCount} coincidencia(s) registrada(s).`,
      );
    } catch (error) {
      setNotice(error instanceof Error ? error.message : "No se pudo ejecutar el motor.");
    }
  };

  const handleProcessRun = async (run: ReglasRun) => {
    setNotice(null);
    try {
      const result = await processRun.mutateAsync(run.id);
      setNotice(
        `Orquestación completada: ${result.succeededCount} éxito(s), ${result.failedCount} fallo(s).`,
      );
    } catch (error) {
      setNotice(error instanceof Error ? error.message : "No se pudo procesar la corrida.");
    }
  };

  return (
    <div className="space-y-6" data-testid="reglas-execution-section">
      <SchedulerPanel />

      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-lg font-semibold text-[var(--deep)]">Ejecución manual</h2>
          <p className="text-sm text-[var(--muted-foreground)]">
            Evalúa todas las reglas activas contra la maestra DGC del tenant.
          </p>
        </div>
        <Button
          type="button"
          label="Ejecutar ahora"
          className="flit-btn-primary"
          loading={triggerRun.isPending}
          disabled={!canExecute}
          onClick={() => void handleManualRun()}
          data-testid="reglas-trigger-run-btn"
        />
      </div>

      {!canExecute ? (
        <div
          className="rounded-2xl border border-[var(--amber-acc)]/40 bg-[var(--amber-acc)]/10 p-4 text-sm text-[var(--deep)]"
          role="status"
        >
          Su rol no permite ejecutar el motor. Se requiere TenantAdmin o SuperAdmin.
        </div>
      ) : null}

      {notice ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--muted)]/30 p-4 text-sm text-[var(--deep)]"
          role="status"
          data-testid="reglas-execution-notice"
        >
          {notice}
        </div>
      ) : null}

      {runsQuery.isLoading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
        >
          Cargando corridas…
        </div>
      ) : null}

      {!runsQuery.isLoading && runsQuery.isError ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudieron cargar las corridas de evaluación.
        </div>
      ) : null}

      {!runsQuery.isLoading && !runsQuery.isError && runs.length === 0 ? (
        <div
          className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-12 text-center"
          data-testid="reglas-runs-empty"
        >
          <p className="text-sm font-medium text-[var(--deep)]">Sin corridas registradas</p>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            Ejecute el motor manualmente o espere el ciclo programado.
          </p>
        </div>
      ) : null}

      {!runsQuery.isLoading && !runsQuery.isError && runs.length > 0 ? (
        <div
          className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
          data-testid="reglas-runs-table"
        >
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                <th className="px-4 py-3">Inicio</th>
                <th className="px-4 py-3">Disparador</th>
                <th className="px-4 py-3">Estado</th>
                <th className="px-4 py-3">Métricas</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {runs.map((run) => {
                const selected = run.id === selectedRunId;
                return (
                  <tr
                    key={run.id}
                    className={`cursor-pointer border-t border-[var(--border)]/60 hover:bg-[var(--muted)]/40 ${
                      selected ? "bg-[var(--tech)]/10" : ""
                    }`}
                    onClick={() => setSelectedRunId(run.id)}
                    data-testid={`reglas-run-row-${run.id}`}
                  >
                    <td className="px-4 py-3 text-[var(--deep)]">{formatDateTime(run.startedAt)}</td>
                    <td className="px-4 py-3 text-[var(--muted-foreground)]">
                      {formatTriggerType(run.triggerType)}
                    </td>
                    <td className="px-4 py-3">{formatRunStatus(run.status)}</td>
                    <td className="px-4 py-3 text-[var(--muted-foreground)]">
                      {formatRunMetrics(run)}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        <Button
                          type="button"
                          label="Ver coincidencias"
                          className="flit-btn-secondary p-button-sm"
                          onClick={(e) => {
                            e.stopPropagation();
                            setSelectedRunId(run.id);
                          }}
                        />
                        {canExecute && run.matchedCount > 0 ? (
                          <Button
                            type="button"
                            label="Procesar PDF+correo"
                            className="flit-btn-primary p-button-sm"
                            loading={processRun.isPending && selectedRunId === run.id}
                            onClick={(e) => {
                              e.stopPropagation();
                              void handleProcessRun(run);
                            }}
                          />
                        ) : null}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      ) : null}

      {selectedRunId ? (
        <div className="space-y-3">
          <h3 className="text-base font-semibold text-[var(--deep)]">Coincidencias de la corrida</h3>

          {matchesQuery.isLoading ? (
            <div
              className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-8 text-center text-sm text-[var(--muted-foreground)]"
              role="status"
            >
              Cargando coincidencias…
            </div>
          ) : null}

          {!matchesQuery.isLoading && matchesQuery.isError ? (
            <div
              className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
              role="alert"
            >
              No se pudieron cargar las coincidencias.
            </div>
          ) : null}

          {!matchesQuery.isLoading && !matchesQuery.isError && matches.length === 0 ? (
            <div
              className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-8 text-center"
              data-testid="reglas-matches-empty"
            >
              <p className="text-sm text-[var(--muted-foreground)]">
                Esta corrida no registró coincidencias regla-comparendo.
              </p>
            </div>
          ) : null}

          {!matchesQuery.isLoading && !matchesQuery.isError && matches.length > 0 ? (
            <div
              className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
              data-testid="reglas-matches-table"
            >
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                    <th className="px-4 py-3">Regla</th>
                    <th className="px-4 py-3">Comparendo</th>
                    <th className="px-4 py-3">Estado</th>
                    <th className="px-4 py-3">Registrada</th>
                    <th className="px-4 py-3">Procesada</th>
                  </tr>
                </thead>
                <tbody>
                  {matches.map((match) => (
                    <tr
                      key={match.id}
                      className="border-t border-[var(--border)]/60 hover:bg-[var(--muted)]/40"
                    >
                      <td className="px-4 py-3 font-medium text-[var(--deep)]">{match.ruleName}</td>
                      <td className="px-4 py-3 text-[var(--muted-foreground)]">
                        {match.comparendoNumero}
                      </td>
                      <td className="px-4 py-3">{formatMatchStatus(match.status)}</td>
                      <td className="px-4 py-3 text-[var(--muted-foreground)]">
                        {formatDateTime(match.createdAt)}
                      </td>
                      <td className="px-4 py-3 text-[var(--muted-foreground)]">
                        {formatDateTime(match.processedAt)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}
