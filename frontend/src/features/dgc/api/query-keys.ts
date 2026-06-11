import type { ComparendoMaestraFilters } from "../lib/comparendo-maestra.types";

export const dgcQueryKeys = {
  comparendosMaestra: (filters: ComparendoMaestraFilters) =>
    ["dgc", "comparendos", "maestra", filters] as const,
  comparendoDetail: (id: string) => ["dgc", "comparendos", "detail", id] as const,
  emailLogs: (comparendoId: string) => ["dgc", "emails", comparendoId] as const,
  emailEvidence: (emailId: string) => ["dgc", "emails", "evidence", emailId] as const,
  allComparendos: () => ["dgc", "comparendos"] as const,
};
