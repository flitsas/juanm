import { useMutation } from "@tanstack/react-query";
import { downloadDerechoPeticionPdf, generateDerechoPeticion } from "./plantillas-api";

export function useDpGeneration() {
  const generate = useMutation({
    mutationFn: ({ comparendoId, templateId }: { comparendoId: string; templateId: string }) =>
      generateDerechoPeticion(comparendoId, templateId),
  });

  const download = useMutation({
    mutationFn: (derechoPeticionId: string) => downloadDerechoPeticionPdf(derechoPeticionId),
  });

  return { generate, download };
}
