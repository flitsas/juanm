"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { UpdateTenantProfilePayload } from "../lib/notif.types";
import { fetchTenantProfile, updateTenantProfile } from "./notif-api";
import { notifQueryKeys } from "./query-keys";

export function useNotifTenantProfile() {
  return useQuery({
    queryKey: notifQueryKeys.profile(),
    queryFn: ({ signal }) => fetchTenantProfile({ signal }),
  });
}

export function useNotifProfileMutations() {
  const queryClient = useQueryClient();

  const update = useMutation({
    mutationFn: (payload: UpdateTenantProfilePayload) => updateTenantProfile(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: notifQueryKeys.profile() });
    },
  });

  return { update };
}
