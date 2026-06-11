"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { SaveRulePayload } from "../lib/notif.types";
import {
  createRule,
  deleteRule,
  fetchQueue,
  fetchRules,
  fetchSwitch,
  updateRule,
  updateSwitch,
} from "./notif-api";
import { notifQueryKeys } from "./query-keys";

export function useNotifRules() {
  return useQuery({
    queryKey: notifQueryKeys.rules(),
    queryFn: ({ signal }) => fetchRules({ signal }),
  });
}

export function useNotifQueue() {
  return useQuery({
    queryKey: notifQueryKeys.queue(),
    queryFn: ({ signal }) => fetchQueue({ signal }),
  });
}

export function useNotifSwitch() {
  return useQuery({
    queryKey: notifQueryKeys.switch(),
    queryFn: ({ signal }) => fetchSwitch({ signal }),
  });
}

export function useNotifRuleMutations() {
  const queryClient = useQueryClient();

  const invalidateRules = () => {
    void queryClient.invalidateQueries({ queryKey: notifQueryKeys.rules() });
  };

  const create = useMutation({
    mutationFn: (payload: SaveRulePayload) => createRule(payload),
    onSuccess: invalidateRules,
  });

  const update = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SaveRulePayload }) =>
      updateRule(id, payload),
    onSuccess: invalidateRules,
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteRule(id),
    onSuccess: invalidateRules,
  });

  const setSwitch = useMutation({
    mutationFn: (dispatchEnabled: boolean) => updateSwitch(dispatchEnabled),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: notifQueryKeys.switch() });
    },
  });

  return { create, update, remove, setSwitch };
}
