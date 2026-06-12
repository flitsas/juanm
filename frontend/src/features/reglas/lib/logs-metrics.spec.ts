import { describe, expect, it } from "vitest";
import { aggregateRunMetrics, countMatchesByStatus } from "./logs-metrics";
import type { ReglasMatch, ReglasRun } from "./reglas.types";

describe("logs-metrics", () => {
  it("agrega métricas de corridas", () => {
    const runs: ReglasRun[] = [
      {
        id: "1",
        startedAt: "2026-06-12T10:00:00Z",
        status: "completed",
        triggerType: "manual",
        evaluatedCount: 10,
        matchedCount: 2,
        processedCount: 1,
        failedCount: 0,
      },
      {
        id: "2",
        startedAt: "2026-06-12T11:00:00Z",
        status: "completed",
        triggerType: "scheduled",
        evaluatedCount: 5,
        matchedCount: 1,
        processedCount: 1,
        failedCount: 1,
      },
    ];
    const metrics = aggregateRunMetrics(runs);
    expect(metrics.totalRuns).toBe(2);
    expect(metrics.totalEvaluated).toBe(15);
    expect(metrics.totalMatched).toBe(3);
  });

  it("cuenta coincidencias por estado", () => {
    const matches: ReglasMatch[] = [
      {
        id: "1",
        dynamicRuleId: "r1",
        ruleName: "DP",
        comparendoId: "c1",
        comparendoNumero: "CMP-1",
        status: "success",
        createdAt: "2026-06-12T10:00:00Z",
      },
      {
        id: "2",
        dynamicRuleId: "r1",
        ruleName: "DP",
        comparendoId: "c2",
        comparendoNumero: "CMP-2",
        status: "matched",
        createdAt: "2026-06-12T10:01:00Z",
      },
    ];
    expect(countMatchesByStatus(matches)).toEqual({ success: 1, matched: 1 });
  });
});
