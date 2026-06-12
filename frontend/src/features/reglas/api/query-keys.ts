export const reglasQueryKeys = {
  all: ["reglas"] as const,
  rules: () => [...reglasQueryKeys.all, "rules"] as const,
  contacts: () => [...reglasQueryKeys.all, "contacts"] as const,
  runs: () => [...reglasQueryKeys.all, "runs"] as const,
  matches: (runId?: string | null, ruleId?: string | null) =>
    [...reglasQueryKeys.all, "matches", runId ?? "all", ruleId ?? "all"] as const,
};
