import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  fetchReglasMatches,
  fetchReglasRuns,
  processReglasRun,
  triggerReglasRun,
} from "./reglas-api";
import { reglasQueryKeys } from "./query-keys";

export function useReglasRuns() {
  return useQuery({
    queryKey: reglasQueryKeys.runs(),
    queryFn: ({ signal }) => fetchReglasRuns({ signal }),
  });
}

export function useReglasMatches(runId?: string | null, ruleId?: string | null) {
  return useQuery({
    queryKey: reglasQueryKeys.matches(runId, ruleId),
    queryFn: ({ signal }) => fetchReglasMatches({ runId: runId ?? undefined, ruleId: ruleId ?? undefined }, { signal }),
    enabled: Boolean(runId),
  });
}

export function useReglasExecutionMutations() {
  const queryClient = useQueryClient();

  const invalidateExecution = () => {
    void queryClient.invalidateQueries({ queryKey: reglasQueryKeys.runs() });
    void queryClient.invalidateQueries({ queryKey: reglasQueryKeys.all });
  };

  const triggerRun = useMutation({
    mutationFn: () => triggerReglasRun(),
    onSuccess: invalidateExecution,
  });

  const processRun = useMutation({
    mutationFn: (runId: string) => processReglasRun(runId),
    onSuccess: invalidateExecution,
  });

  return { triggerRun, processRun };
}
