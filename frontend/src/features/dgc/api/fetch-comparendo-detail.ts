import { getApiBaseUrl } from "@/lib/api-base-url";
import type { ComparendoMaestraItem } from "../lib/comparendo-maestra.types";
import { getTenantId } from "../lib/tenant-context";

export async function fetchComparendoDetail(id: string): Promise<ComparendoMaestraItem> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/dgc/comparendos/${id}`, {
    headers: {
      "X-Tenant-Id": getTenantId(),
      Accept: "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`DGC detail request failed (${response.status})`);
  }

  return response.json() as Promise<ComparendoMaestraItem>;
}
