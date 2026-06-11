import { getApiBaseUrl } from "@/lib/api-base-url";
import type { EmailEvidence } from "../lib/detail-panel.types";
import { getTenantId } from "../lib/tenant-context";

export async function fetchEmailEvidence(emailId: string): Promise<EmailEvidence> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/dgc/emails/${emailId}/evidence`, {
    headers: {
      "X-Tenant-Id": getTenantId(),
      Accept: "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(`DGC email evidence request failed (${response.status})`);
  }

  return response.json() as Promise<EmailEvidence>;
}
