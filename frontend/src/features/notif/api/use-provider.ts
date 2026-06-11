"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { SaveProviderPayload, TestProviderPayload } from "../lib/notif.types";
import { fetchProvider, saveProvider, testProvider } from "./notif-api";
import { notifQueryKeys } from "./query-keys";

export function useNotifProvider() {
  return useQuery({
    queryKey: notifQueryKeys.provider(),
    queryFn: ({ signal }) => fetchProvider({ signal }),
  });
}

export function useNotifProviderMutations() {
  const queryClient = useQueryClient();

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: notifQueryKeys.provider() });
  };

  const save = useMutation({
    mutationFn: (payload: SaveProviderPayload) => saveProvider(payload),
    onSuccess: invalidate,
  });

  const test = useMutation({
    mutationFn: (payload: TestProviderPayload) => testProvider(payload),
  });

  return { save, test };
}
