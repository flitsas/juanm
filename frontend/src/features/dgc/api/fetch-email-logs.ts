import { getApiBaseUrl } from "@/lib/api-base-url";
import type { EmailLogListResult } from "../lib/detail-panel.types";
import { getTenantId } from "../lib/tenant-context";

export async function fetchEmailLogs(comparendoId: string): Promise<EmailLogListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/dgc/comparendos/${comparendoId}/emails`, {
    headers: {
      "X-Tenant-Id": getTenantId(),
      Accept: "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`DGC email log request failed (${response.status})`);
  }

  return response.json() as Promise<EmailLogListResult>;
}
