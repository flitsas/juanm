"use client";

import { useReglasMatches, useReglasRuns } from "../api/use-runs";
import { formatMatchStatus, formatRunMetrics, formatRunStatus, formatTriggerType } from "../lib/execution-labels";
import { aggregateRunMetrics, countMatchesByStatus } from "../lib/logs-metrics";

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

export function LogsSection() {
  const runsQuery = useReglasRuns();
  const matchesQuery = useReglasMatches(null);
  const runs = runsQuery.data?.items ?? [];
  const matches = matchesQuery.data?.items ?? [];
  const metrics = aggregateRunMetrics(runs);
  const statusCounts = countMatchesByStatus(matches);

  const loading = runsQuery.isLoading || matchesQuery.isLoading;

  return (
    <div className="space-y-6" data-testid="reglas-logs-section">
      {loading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-8 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
        >
          Cargando métricas y trazabilidad…
        </div>
      ) : null}

      {!loading && (runsQuery.isError || matchesQuery.isError) ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudieron cargar los logs de ejecución.
        </div>
      ) : null}

      {!loading && !runsQuery.isError && !matchesQuery.isError ? (
        <>
          <div
            className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5"
            data-testid="reglas-logs-metrics"
          >
            <MetricCard label="Corridas" value={metrics.totalRuns} />
            <MetricCard label="Evaluados" value={metrics.totalEvaluated} />
            <MetricCard label="Coincidencias" value={metrics.totalMatched} />
            <MetricCard label="Procesados" value={metrics.totalProcessed} />
            <MetricCard label="Fallos" value={metrics.totalFailed} />
          </div>

          {Object.keys(statusCounts).length > 0 ? (
            <div className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-4 text-sm">
              <p className="font-medium text-[var(--deep)]">Procesamiento por estado</p>
              <ul className="mt-2 flex flex-wrap gap-3 text-[var(--muted-foreground)]">
                {Object.entries(statusCounts).map(([status, count]) => (
                  <li key={status}>
                    {formatMatchStatus(status)}: <strong>{count}</strong>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

          <div>
            <h3 className="mb-3 text-base font-semibold text-[var(--deep)]">Historial de corridas</h3>
            {runs.length === 0 ? (
              <div
                className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-8 text-center text-sm text-[var(--muted-foreground)]"
              >
                Sin corridas registradas.
              </div>
            ) : (
              <div className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]">
                <table className="w-full border-collapse text-sm">
                  <thead>
                    <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                      <th className="px-4 py-3">Inicio</th>
                      <th className="px-4 py-3">Disparador</th>
                      <th className="px-4 py-3">Estado</th>
                      <th className="px-4 py-3">Métricas</th>
                    </tr>
                  </thead>
                  <tbody>
                    {runs.map((run) => (
                      <tr key={run.id} className="border-t border-[var(--border)]/60">
                        <td className="px-4 py-3">{formatDateTime(run.startedAt)}</td>
                        <td className="px-4 py-3">{formatTriggerType(run.triggerType)}</td>
                        <td className="px-4 py-3">{formatRunStatus(run.status)}</td>
                        <td className="px-4 py-3 text-[var(--muted-foreground)]">
                          {formatRunMetrics(run)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          <div>
            <h3 className="mb-3 text-base font-semibold text-[var(--deep)]">
              Trazabilidad regla-comparendo
            </h3>
            {matches.length === 0 ? (
              <div
                className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-8 text-center text-sm text-[var(--muted-foreground)]"
                data-testid="reglas-logs-matches-empty"
              >
                Sin registros de procesamiento.
              </div>
            ) : (
              <div
                className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
                data-testid="reglas-logs-matches-table"
              >
                <table className="w-full border-collapse text-sm">
                  <thead>
                    <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                      <th className="px-4 py-3">Regla</th>
                      <th className="px-4 py-3">Comparendo</th>
                      <th className="px-4 py-3">Estado</th>
                      <th className="px-4 py-3">Registrado</th>
                      <th className="px-4 py-3">Procesado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {matches.map((match) => (
                      <tr key={match.id} className="border-t border-[var(--border)]/60">
                        <td className="px-4 py-3 font-medium text-[var(--deep)]">{match.ruleName}</td>
                        <td className="px-4 py-3">{match.comparendoNumero}</td>
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
            )}
          </div>
        </>
      ) : null}
    </div>
  );
}

function MetricCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-4 shadow-sm">
      <p className="text-xs text-[var(--muted-foreground)]">{label}</p>
      <p className="mt-1 text-2xl font-bold text-[var(--deep)]">{value}</p>
    </div>
  );
}
