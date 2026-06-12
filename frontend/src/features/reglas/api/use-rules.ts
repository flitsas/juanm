import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { SaveReglasRulePayload } from "../lib/reglas.types";
import { reglasQueryKeys } from "./query-keys";
import {
  createReglasRule,
  deleteReglasRule,
  fetchReglasContacts,
  fetchReglasRules,
  updateReglasRule,
} from "./reglas-api";

export function useReglasRules() {
  return useQuery({
    queryKey: reglasQueryKeys.rules(),
    queryFn: ({ signal }) => fetchReglasRules({ signal }),
  });
}

export function useReglasContacts() {
  return useQuery({
    queryKey: reglasQueryKeys.contacts(),
    queryFn: ({ signal }) => fetchReglasContacts({ signal }),
  });
}

export function useReglasRuleMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: reglasQueryKeys.rules() });
  };

  const create = useMutation({
    mutationFn: (payload: SaveReglasRulePayload) => createReglasRule(payload),
    onSuccess: invalidate,
  });

  const update = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SaveReglasRulePayload }) =>
      updateReglasRule(id, payload),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteReglasRule(id),
    onSuccess: invalidate,
  });

  return { create, update, remove };
}
