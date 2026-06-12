import { describe, expect, it } from "vitest";
import {
  canExecuteReglas,
  formatMatchStatus,
  formatRunMetrics,
  formatRunStatus,
  formatTriggerType,
} from "./execution-labels";
import type { ReglasRun } from "./reglas.types";

describe("execution-labels", () => {
  it("traduce estados de corrida y disparador", () => {
    expect(formatRunStatus("completed")).toBe("Completada");
    expect(formatTriggerType("manual")).toBe("Manual");
    expect(formatMatchStatus("matched")).toBe("Coincidencia");
  });

  it("resume métricas de corrida", () => {
    const run: ReglasRun = {
      id: "1",
      startedAt: "2026-06-12T10:00:00Z",
      status: "completed",
      triggerType: "manual",
      evaluatedCount: 10,
      matchedCount: 2,
      processedCount: 1,
      failedCount: 0,
    };
    expect(formatRunMetrics(run)).toContain("10 eval.");
    expect(formatRunMetrics(run)).toContain("2 coincid.");
  });

  it("permite ejecución solo a TenantAdmin y SuperAdmin", () => {
    expect(canExecuteReglas("TenantAdmin")).toBe(true);
    expect(canExecuteReglas("SuperAdmin")).toBe(true);
    expect(canExecuteReglas("Operator")).toBe(false);
  });
});
