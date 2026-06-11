import { getApiBaseUrl } from "@/lib/api-base-url";
import type {
  CreateCompanyPayload,
  NotifCompany,
  NotifCompanyListResult,
  NotifProvider,
  SaveProviderPayload,
  TestProviderPayload,
  TestProviderResult,
  UpdateCompanyPayload,
} from "../lib/notif.types";
import { buildNotifHeaders } from "./notif-headers";

async function parseError(response: Response): Promise<Error> {
  try {
    const body = (await response.json()) as { message?: string };
    return new Error(body.message ?? `NOTIF request failed (${response.status})`);
  } catch {
    return new Error(`NOTIF request failed (${response.status})`);
  }
}

export async function fetchCompanies(init?: RequestInit): Promise<NotifCompanyListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/companies`, {
    ...init,
    headers: {
      ...buildNotifHeaders({ superAdmin: true }),
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifCompanyListResult>;
}

export async function createCompany(
  payload: CreateCompanyPayload,
  init?: RequestInit,
): Promise<NotifCompany> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/companies`, {
    method: "POST",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildNotifHeaders({ superAdmin: true }),
      ...init?.headers,
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifCompany>;
}

export async function updateCompany(
  id: string,
  payload: UpdateCompanyPayload,
  init?: RequestInit,
): Promise<NotifCompany> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/companies/${id}`, {
    method: "PUT",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildNotifHeaders({ superAdmin: true }),
      ...init?.headers,
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifCompany>;
}

export async function deleteCompany(id: string, init?: RequestInit): Promise<void> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/companies/${id}`, {
    method: "DELETE",
    ...init,
    headers: {
      ...buildNotifHeaders({ superAdmin: true }),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw await parseError(response);
  }
}

export async function fetchProvider(init?: RequestInit): Promise<NotifProvider | null> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/provider`, {
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifProvider>;
}

export async function saveProvider(
  payload: SaveProviderPayload,
  init?: RequestInit,
): Promise<NotifProvider> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/provider`, {
    method: "PUT",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifProvider>;
}

export async function testProvider(
  payload: TestProviderPayload,
  init?: RequestInit,
): Promise<TestProviderResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/provider/test`, {
    method: "POST",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<TestProviderResult>;
}
