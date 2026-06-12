import { getApiBaseUrl } from "@/lib/api-base-url";
import type {
  ReglasContactListResult,
  ReglasRule,
  ReglasRuleListResult,
  SaveReglasRulePayload,
} from "../lib/reglas.types";
import { buildReglasHeaders } from "./reglas-headers";

async function parseError(response: Response): Promise<Error> {
  try {
    const body = (await response.json()) as { message?: string; code?: string };
    return new Error(body.message ?? `REGLAS request failed (${response.status})`);
  } catch {
    return new Error(`REGLAS request failed (${response.status})`);
  }
}

export async function fetchReglasRules(init?: RequestInit): Promise<ReglasRuleListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/rules`, {
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
    cache: "no-store",
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasRuleListResult>;
}

export async function createReglasRule(
  payload: SaveReglasRulePayload,
  init?: RequestInit,
): Promise<ReglasRule> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/rules`, {
    method: "POST",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildReglasHeaders(),
      ...init?.headers,
    },
    body: JSON.stringify(payload),
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasRule>;
}

export async function updateReglasRule(
  id: string,
  payload: SaveReglasRulePayload,
  init?: RequestInit,
): Promise<ReglasRule> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/rules/${id}`, {
    method: "PUT",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildReglasHeaders(),
      ...init?.headers,
    },
    body: JSON.stringify(payload),
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasRule>;
}

export async function deleteReglasRule(id: string, init?: RequestInit): Promise<void> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/rules/${id}`, {
    method: "DELETE",
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
  });
  if (!response.ok) throw await parseError(response);
}

export async function fetchReglasContacts(init?: RequestInit): Promise<ReglasContactListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/contacts`, {
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
    cache: "no-store",
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasContactListResult>;
}
