import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { UpdateFieldMappingsRequest } from "../lib/plantillas.types";
import {
  activatePdfTemplate,
  fetchPdfTemplate,
  fetchPdfTemplates,
  fetchSystemVariables,
  updateFieldMappings,
  uploadPdfTemplate,
} from "./plantillas-api";
import { plantillasQueryKeys } from "./query-keys";

export function usePdfTemplates() {
  return useQuery({
    queryKey: plantillasQueryKeys.templates(),
    queryFn: ({ signal }) => fetchPdfTemplates({ signal }),
  });
}

export function usePdfTemplate(id: string | null) {
  return useQuery({
    queryKey: plantillasQueryKeys.template(id ?? ""),
    queryFn: ({ signal }) => {
      if (!id) throw new Error("Template id is required");
      return fetchPdfTemplate(id, { signal });
    },
    enabled: Boolean(id),
  });
}

export function useSystemVariables() {
  return useQuery({
    queryKey: plantillasQueryKeys.systemVariables(),
    queryFn: ({ signal }) => fetchSystemVariables({ signal }),
  });
}

export function usePlantillaMutations() {
  const queryClient = useQueryClient();

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: plantillasQueryKeys.all });
  };

  const upload = useMutation({
    mutationFn: ({ file, name }: { file: File; name?: string }) => uploadPdfTemplate(file, name),
    onSuccess: invalidate,
  });

  const saveMappings = useMutation({
    mutationFn: ({ templateId, body }: { templateId: string; body: UpdateFieldMappingsRequest }) =>
      updateFieldMappings(templateId, body),
    onSuccess: invalidate,
  });

  const activate = useMutation({
    mutationFn: (templateId: string) => activatePdfTemplate(templateId),
    onSuccess: invalidate,
  });

  return { upload, saveMappings, activate };
}
