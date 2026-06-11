import { getApiBaseUrl } from "@/lib/api-base-url";
import type { OcrBufferForm, OcrLoteUploadResult } from "../lib/ocr.types";
import { getTenantId } from "../lib/tenant-context";

function tenantHeaders(): HeadersInit {
  return {
    "X-Tenant-Id": getTenantId(),
    Accept: "application/json",
  };
}

export async function uploadOcrLote(files: File[]): Promise<OcrLoteUploadResult> {
  const formData = new FormData();
  for (const file of files) {
    formData.append("files", file);
  }

  const response = await fetch(`${getApiBaseUrl()}/api/v1/dgc/ocr/lotes`, {
    method: "POST",
    headers: tenantHeaders(),
    body: formData,
  });

  if (!response.ok) {
    const detail = await response.text();
    throw new Error(`OCR upload failed (${response.status}): ${detail.slice(0, 200)}`);
  }

  return response.json() as Promise<OcrLoteUploadResult>;
}

export async function confirmOcrItem(form: OcrBufferForm): Promise<{ comparendoId: string }> {
  const body = {
    numeroComparendo: form.numeroComparendo.trim(),
    estado: form.estado.trim(),
    infractorNombre: form.infractorNombre.trim() || null,
    documento: form.documento.trim() || null,
    placa: form.placa.trim() || null,
    infraccionCodigo: form.infraccionCodigo.trim() || null,
    fechaComparendo: form.fechaComparendo || null,
    totalValor: Number.parseFloat(form.totalValor) || 0,
  };

  const response = await fetch(`${getApiBaseUrl()}/api/v1/dgc/ocr/items/${form.itemId}/confirm`, {
    method: "POST",
    headers: {
      ...tenantHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  if (response.status === 409) {
    const payload = (await response.json()) as { code?: string; message?: string };
    throw new Error(payload.message ?? payload.code ?? "DGC_DUPLICATE");
  }

  if (!response.ok) {
    throw new Error(`OCR confirm failed (${response.status})`);
  }

  const result = (await response.json()) as { comparendoId: string };
  return result;
}
