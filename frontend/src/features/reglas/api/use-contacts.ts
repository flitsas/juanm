import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { SaveReglasContactPayload } from "../lib/reglas.types";
import { createReglasContact, deleteReglasContact, updateReglasContact } from "./reglas-api";
import { reglasQueryKeys } from "./query-keys";

export function useReglasContactMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: reglasQueryKeys.contacts() });
  };

  const create = useMutation({
    mutationFn: (payload: SaveReglasContactPayload) => createReglasContact(payload),
    onSuccess: invalidate,
  });

  const update = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SaveReglasContactPayload }) =>
      updateReglasContact(id, payload),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteReglasContact(id),
    onSuccess: invalidate,
  });

  return { create, update, remove };
}
