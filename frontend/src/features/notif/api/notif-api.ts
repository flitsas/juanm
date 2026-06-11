import { getApiBaseUrl } from "@/lib/api-base-url";
import type {
  CreateCompanyPayload,
  NotifCompany,
  NotifCompanyListResult,
  NotifProvider,
  NotifQueueListResult,
  NotifRule,
  NotifRuleListResult,
  NotifSwitchState,
  NotifTemplate,
  NotifTemplateListResult,
  PreviewTemplateResult,
  SaveProviderPayload,
  SaveRulePayload,
  SaveTemplatePayload,
  TenantProfile,
  TestProviderPayload,
  TestProviderResult,
  UpdateCompanyPayload,
  UpdateTenantProfilePayload,
  UploadTemplateAssetResult,
} from "../lib/notif.types";
import { DEFAULT_TEMPLATE_VARIABLES } from "../lib/template-preview";
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

export async function fetchTemplates(init?: RequestInit): Promise<NotifTemplateListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/templates`, {
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifTemplateListResult>;
}

export async function createTemplate(
  payload: SaveTemplatePayload,
  init?: RequestInit,
): Promise<NotifTemplate> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/templates`, {
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

  return response.json() as Promise<NotifTemplate>;
}

export async function updateTemplate(
  id: string,
  payload: SaveTemplatePayload,
  init?: RequestInit,
): Promise<NotifTemplate> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/templates/${id}`, {
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

  return response.json() as Promise<NotifTemplate>;
}

export async function deleteTemplate(id: string, init?: RequestInit): Promise<void> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/templates/${id}`, {
    method: "DELETE",
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw await parseError(response);
  }
}

export async function previewTemplate(
  id: string,
  variables: Record<string, string> = DEFAULT_TEMPLATE_VARIABLES,
  init?: RequestInit,
): Promise<PreviewTemplateResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/templates/${id}/preview`, {
    method: "POST",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    body: JSON.stringify({ variables }),
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<PreviewTemplateResult>;
}

export async function uploadTemplateAsset(
  file: File,
  init?: RequestInit,
): Promise<UploadTemplateAssetResult> {
  const form = new FormData();
  form.append("file", file);

  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/templates/assets`, {
    method: "POST",
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    body: form,
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<UploadTemplateAssetResult>;
}

export async function fetchTenantProfile(init?: RequestInit): Promise<TenantProfile> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/profile`, {
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<TenantProfile>;
}

export async function updateTenantProfile(
  payload: UpdateTenantProfilePayload,
  init?: RequestInit,
): Promise<TenantProfile> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/profile`, {
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

  return response.json() as Promise<TenantProfile>;
}

export async function fetchRules(init?: RequestInit): Promise<NotifRuleListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/rules`, {
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifRuleListResult>;
}

export async function createRule(payload: SaveRulePayload, init?: RequestInit): Promise<NotifRule> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/rules`, {
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

  return response.json() as Promise<NotifRule>;
}

export async function updateRule(
  id: string,
  payload: SaveRulePayload,
  init?: RequestInit,
): Promise<NotifRule> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/rules/${id}`, {
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

  return response.json() as Promise<NotifRule>;
}

export async function deleteRule(id: string, init?: RequestInit): Promise<void> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/rules/${id}`, {
    method: "DELETE",
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw await parseError(response);
  }
}

export async function fetchQueue(init?: RequestInit): Promise<NotifQueueListResult> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/queue`, {
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifQueueListResult>;
}

export async function fetchSwitch(init?: RequestInit): Promise<NotifSwitchState> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/switch`, {
    ...init,
    headers: {
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifSwitchState>;
}

export async function updateSwitch(
  dispatchEnabled: boolean,
  init?: RequestInit,
): Promise<NotifSwitchState> {
  const response = await fetch(`${getApiBaseUrl()}/api/v1/notif/switch`, {
    method: "PUT",
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...buildNotifHeaders(),
      ...init?.headers,
    },
    body: JSON.stringify({ dispatchEnabled }),
  });

  if (!response.ok) {
    throw await parseError(response);
  }

  return response.json() as Promise<NotifSwitchState>;
}
