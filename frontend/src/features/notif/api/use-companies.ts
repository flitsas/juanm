"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { CreateCompanyPayload, UpdateCompanyPayload } from "../lib/notif.types";
import { createCompany, deleteCompany, fetchCompanies, updateCompany } from "./notif-api";
import { notifQueryKeys } from "./query-keys";

export function useNotifCompanies() {
  return useQuery({
    queryKey: notifQueryKeys.companies(),
    queryFn: ({ signal }) => fetchCompanies({ signal }),
  });
}

export function useNotifCompanyMutations() {
  const queryClient = useQueryClient();

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: notifQueryKeys.companies() });
  };

  const create = useMutation({
    mutationFn: (payload: CreateCompanyPayload) => createCompany(payload),
    onSuccess: invalidate,
  });

  const update = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateCompanyPayload }) =>
      updateCompany(id, payload),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteCompany(id),
    onSuccess: invalidate,
  });

  return { create, update, remove };
}
