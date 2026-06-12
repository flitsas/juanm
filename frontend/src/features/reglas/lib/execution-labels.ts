import type { ReglasMatch, ReglasRun } from "./reglas.types";

const RUN_STATUS_LABELS: Record<string, string> = {
  running: "En curso",
  completed: "Completada",
  failed: "Fallida",
};

const TRIGGER_LABELS: Record<string, string> = {
  manual: "Manual",
  scheduled: "Programada",
};

const MATCH_STATUS_LABELS: Record<string, string> = {
  matched: "Coincidencia",
  success: "Procesada",
  failed: "Fallida",
  skipped: "Omitida",
};

export function formatRunStatus(status: string): string {
  return RUN_STATUS_LABELS[status] ?? status;
}

export function formatTriggerType(triggerType: string): string {
  return TRIGGER_LABELS[triggerType] ?? triggerType;
}

export function formatMatchStatus(status: string): string {
  return MATCH_STATUS_LABELS[status] ?? status;
}

export function formatRunMetrics(run: ReglasRun): string {
  return `${run.evaluatedCount} eval. · ${run.matchedCount} coincid. · ${run.processedCount} proc. · ${run.failedCount} fallos`;
}

export function formatMatchRow(match: ReglasMatch): string {
  return `${match.ruleName} → ${match.comparendoNumero}`;
}

export function getSchedulerDisplayConfig(): {
  enabled: boolean;
  pollIntervalSeconds: number;
  processMatchesAfterEvaluation: boolean;
} {
  const enabledRaw = process.env.NEXT_PUBLIC_REGLAS_EXECUTION_ENABLED ?? "true";
  const intervalRaw = process.env.NEXT_PUBLIC_REGLAS_POLL_INTERVAL_SECONDS ?? "60";
  const processRaw = process.env.NEXT_PUBLIC_REGLAS_PROCESS_AFTER_EVAL ?? "true";

  return {
    enabled: enabledRaw.toLowerCase() === "true",
    pollIntervalSeconds: Number.parseInt(intervalRaw, 10) || 60,
    processMatchesAfterEvaluation: processRaw.toLowerCase() === "true",
  };
}

export function canExecuteReglas(role: string | undefined): boolean {
  if (!role) return false;
  const normalized = role.toLowerCase();
  return (
    normalized === "tenantadmin" || normalized === "superadmin" || normalized === "super_admin"
  );
}
