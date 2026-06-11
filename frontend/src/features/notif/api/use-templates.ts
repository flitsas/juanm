"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { SaveTemplatePayload } from "../lib/notif.types";
import {
  createTemplate,
  deleteTemplate,
  fetchTemplates,
  previewTemplate,
  updateTemplate,
  uploadTemplateAsset,
} from "./notif-api";
import { notifQueryKeys } from "./query-keys";

export function useNotifTemplates() {
  return useQuery({
    queryKey: notifQueryKeys.templates(),
    queryFn: ({ signal }) => fetchTemplates({ signal }),
  });
}

export function useNotifTemplateMutations() {
  const queryClient = useQueryClient();

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: notifQueryKeys.templates() });
  };

  const create = useMutation({
    mutationFn: (payload: SaveTemplatePayload) => createTemplate(payload),
    onSuccess: invalidate,
  });

  const update = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SaveTemplatePayload }) =>
      updateTemplate(id, payload),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (id: string) => deleteTemplate(id),
    onSuccess: invalidate,
  });

  const preview = useMutation({
    mutationFn: ({ id, variables }: { id: string; variables?: Record<string, string> }) =>
      previewTemplate(id, variables),
  });

  const uploadAsset = useMutation({
    mutationFn: (file: File) => uploadTemplateAsset(file),
  });

  return { create, update, remove, preview, uploadAsset };
}
