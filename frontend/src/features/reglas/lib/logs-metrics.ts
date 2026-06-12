import type { ReglasMatch, ReglasRun } from "./reglas.types";

export type ReglasAggregateMetrics = {
  totalRuns: number;
  totalEvaluated: number;
  totalMatched: number;
  totalProcessed: number;
  totalFailed: number;
};

export function aggregateRunMetrics(runs: ReglasRun[]): ReglasAggregateMetrics {
  return runs.reduce(
    (acc, run) => ({
      totalRuns: acc.totalRuns + 1,
      totalEvaluated: acc.totalEvaluated + run.evaluatedCount,
      totalMatched: acc.totalMatched + run.matchedCount,
      totalProcessed: acc.totalProcessed + run.processedCount,
      totalFailed: acc.totalFailed + run.failedCount,
    }),
    {
      totalRuns: 0,
      totalEvaluated: 0,
      totalMatched: 0,
      totalProcessed: 0,
      totalFailed: 0,
    },
  );
}

export function countMatchesByStatus(matches: ReglasMatch[]): Record<string, number> {
  return matches.reduce<Record<string, number>>((acc, match) => {
    acc[match.status] = (acc[match.status] ?? 0) + 1;
    return acc;
  }, {});
}
