"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchEmailEvidence } from "./fetch-email-evidence";
import { fetchEmailLogs } from "./fetch-email-logs";
import { dgcQueryKeys } from "./query-keys";

export function useEmailLogs(comparendoId: string | null) {
  return useQuery({
    queryKey: comparendoId ? dgcQueryKeys.emailLogs(comparendoId) : ["dgc", "emails", "idle"],
    queryFn: () => {
      if (!comparendoId) {
        throw new Error("comparendoId is required");
      }
      return fetchEmailLogs(comparendoId);
    },
    enabled: Boolean(comparendoId),
  });
}

export function useEmailEvidence(emailId: string | null, enabled: boolean) {
  return useQuery({
    queryKey: emailId ? dgcQueryKeys.emailEvidence(emailId) : ["dgc", "emails", "evidence", "idle"],
    queryFn: () => {
      if (!emailId) {
        throw new Error("emailId is required");
      }
      return fetchEmailEvidence(emailId);
    },
    enabled: enabled && Boolean(emailId),
  });
}
