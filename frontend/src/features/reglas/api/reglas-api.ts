import { getApiBaseUrl } from "@/lib/api-base-url";
import type {
  ReglasContactListResult,
  ReglasMatchListResult,
  ReglasProcessResult,
  ReglasRule,
  ReglasRuleListResult,
  ReglasRun,
  ReglasRunListResult,
  ReglasSecretariatContact,
  SaveReglasContactPayload,
  SaveReglasRulePayload,
  TriggerReglasRunResult,
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

export async function createReglasContact(
  payload: SaveReglasContactPayload,
  init?: RequestInit,
): Promise<ReglasSecretariatContact> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/contacts`, {
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
  return response.json() as Promise<ReglasSecretariatContact>;
}

export async function updateReglasContact(
  id: string,
  payload: SaveReglasContactPayload,
  init?: RequestInit,
): Promise<ReglasSecretariatContact> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/contacts/${id}`, {
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
  return response.json() as Promise<ReglasSecretariatContact>;
}

export async function deleteReglasContact(id: string, init?: RequestInit): Promise<void> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/contacts/${id}`, {
    method: "DELETE",
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
  });
  if (!response.ok) throw await parseError(response);
}

export async function fetchReglasRuns(init?: RequestInit): Promise<ReglasRunListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/runs`, {
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
    cache: "no-store",
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasRunListResult>;
}

export async function triggerReglasRun(init?: RequestInit): Promise<TriggerReglasRunResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/runs`, {
    method: "POST",
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<TriggerReglasRunResult>;
}

export async function fetchReglasRun(id: string, init?: RequestInit): Promise<ReglasRun> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/runs/${id}`, {
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
    cache: "no-store",
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasRun>;
}

export async function fetchReglasMatches(
  params?: { runId?: string; ruleId?: string },
  init?: RequestInit,
): Promise<ReglasMatchListResult> {
  const search = new URLSearchParams();
  if (params?.runId) search.set("runId", params.runId);
  if (params?.ruleId) search.set("ruleId", params.ruleId);
  const qs = search.toString();
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/matches${qs ? `?${qs}` : ""}`, {
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
    cache: "no-store",
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasMatchListResult>;
}

export async function processReglasRun(
  id: string,
  init?: RequestInit,
): Promise<ReglasProcessResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/reglas/runs/${id}/process`, {
    method: "POST",
    ...init,
    headers: { ...buildReglasHeaders(), ...init?.headers },
  });
  if (!response.ok) throw await parseError(response);
  return response.json() as Promise<ReglasProcessResult>;
}
