import { getApiBaseUrl } from "@/lib/api-base-url";
import type { ContraventorForm } from "../lib/detail-panel.types";
import { getTenantId } from "../lib/tenant-context";

export type ContraventorResponse = {
  id: string;
  comparendoId: string;
  nombre: string;
  documento: string;
  correo: string | null;
  asociacionAutomatica: boolean;
};

export async function upsertContraventor(
  comparendoId: string,
  form: ContraventorForm,
): Promise<ContraventorResponse> {
  const response = await fetch(
    `${getApiBaseUrl()}/api/v1/dgc/comparendos/${comparendoId}/contraventor`,
    {
      method: "PUT",
      headers: {
        "X-Tenant-Id": getTenantId(),
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        nombre: form.nombre.trim(),
        documento: form.documento.trim(),
        correo: form.correo.trim() || null,
      }),
    },
  );

  if (!response.ok) {
    throw new Error(`DGC contraventor upsert failed (${response.status})`);
  }

  return response.json() as Promise<ContraventorResponse>;
}
