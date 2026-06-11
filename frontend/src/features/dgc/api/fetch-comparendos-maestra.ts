import { getApiBaseUrl } from "@/lib/api-base-url";
import { buildComparendosQuery } from "../lib/build-comparendos-query";
import type {
  ComparendoMaestraFilters,
  ComparendoMaestraListResult,
} from "../lib/comparendo-maestra.types";
import { getTenantId } from "../lib/tenant-context";

export async function fetchComparendosMaestra(
  filters: ComparendoMaestraFilters,
  init?: RequestInit,
): Promise<ComparendoMaestraListResult> {
  const query = buildComparendosQuery(filters);
  const url = `${getApiBaseUrl()}/api/v1/dgc/comparendos?${query}`;

  const response = await fetch(url, {
    ...init,
    headers: {
      "X-Tenant-Id": getTenantId(),
      Accept: "application/json",
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`DGC maestra request failed (${response.status})`);
  }

  return response.json() as Promise<ComparendoMaestraListResult>;
}
