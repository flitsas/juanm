import { getApiBaseUrl } from "@/lib/api-base-url";
import type {
  InviteUserPayload,
  InviteUserResult,
  RbacAssignmentUpdate,
  RbacMatrix,
  UserSummary,
} from "../types";

async function authFetch<T>(
  path: string,
  accessToken: string,
  init?: RequestInit,
): Promise<T> {
  const res = await fetch(`${getApiBaseUrl()}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
      ...(init?.headers ?? {}),
    },
  });

  if (!res.ok) {
    const body = (await res.json().catch(() => ({}))) as { message?: string };
    throw new Error(body.message ?? "Error en la solicitud.");
  }

  if (res.status === 204) {
    return undefined as T;
  }

  return res.json() as Promise<T>;
}

export async function listAdminUsers(accessToken: string): Promise<UserSummary[]> {
  return authFetch<UserSummary[]>("/auth/admin/users", accessToken);
}

export async function inviteUser(
  accessToken: string,
  payload: InviteUserPayload,
): Promise<InviteUserResult> {
  return authFetch<InviteUserResult>("/auth/users/invite", accessToken, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function getRbacMatrix(accessToken: string): Promise<RbacMatrix> {
  return authFetch<RbacMatrix>("/auth/rbac/matrix", accessToken);
}

export async function updateRbacMatrix(
  accessToken: string,
  assignments: RbacAssignmentUpdate[],
): Promise<void> {
  await authFetch<void>("/auth/rbac/matrix", accessToken, {
    method: "PUT",
    body: JSON.stringify({ assignments }),
  });
}
