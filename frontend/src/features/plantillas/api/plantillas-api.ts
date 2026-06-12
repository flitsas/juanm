import { getApiBaseUrl } from "@/lib/api-base-url";
import type {
  PdfTemplateDetail,
  PdfTemplateListResponse,
  SystemVariableListResponse,
  UpdateFieldMappingsRequest,
  UploadPdfTemplateResponse,
} from "../lib/plantillas.types";
import { buildGdcHeaders } from "./gdc-headers";

async function parseJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    const message =
      typeof body.message === "string" ? body.message : `Error HTTP ${response.status}`;
    throw new Error(message);
  }
  return response.json() as Promise<T>;
}

export async function fetchPdfTemplates(init?: RequestInit): Promise<PdfTemplateListResponse> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/gdc/templates`, {
    ...init,
    headers: buildGdcHeaders(),
    cache: "no-store",
  });
  return parseJson<PdfTemplateListResponse>(response);
}

export async function fetchPdfTemplate(id: string, init?: RequestInit): Promise<PdfTemplateDetail> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/gdc/templates/${id}`, {
    ...init,
    headers: buildGdcHeaders(),
    cache: "no-store",
  });
  return parseJson<PdfTemplateDetail>(response);
}

export async function fetchSystemVariables(
  init?: RequestInit,
): Promise<SystemVariableListResponse> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/gdc/system-variables`, {
    ...init,
    headers: buildGdcHeaders(),
    cache: "no-store",
  });
  return parseJson<SystemVariableListResponse>(response);
}

export async function uploadPdfTemplate(
  file: File,
  name?: string,
): Promise<UploadPdfTemplateResponse> {
  const form = new FormData();
  form.append("file", file);
  if (name?.trim()) {
    form.append("name", name.trim());
  }

  const response = await fetch(`${getApiBaseUrl()}/api/v1/gdc/templates/upload`, {
    method: "POST",
    headers: buildGdcHeaders(),
    body: form,
  });
  return parseJson<UploadPdfTemplateResponse>(response);
}

export async function updateFieldMappings(
  templateId: string,
  body: UpdateFieldMappingsRequest,
): Promise<PdfTemplateDetail> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/gdc/templates/${templateId}/fields`, {
    method: "PUT",
    headers: buildGdcHeaders({ json: true }),
    body: JSON.stringify(body),
  });
  return parseJson<PdfTemplateDetail>(response);
}

export async function activatePdfTemplate(
  templateId: string,
): Promise<{ id: string; isActive: boolean }> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/gdc/templates/${templateId}/activate`, {
    method: "POST",
    headers: buildGdcHeaders(),
  });
  return parseJson<{ id: string; isActive: boolean }>(response);
}
