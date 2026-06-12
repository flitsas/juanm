import { getApiBaseUrl } from "@/lib/api-base-url";
import type {
  ActivateAccountPayload,
  ActivateAccountResult,
  AuthSession,
  LoginPayload,
} from "../types";

export async function login(payload: LoginPayload): Promise<AuthSession> {
  const res = await fetch(`${getApiBaseUrl()}/api/v1/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const body = (await res.json().catch(() => ({}))) as { message?: string };
    throw new Error(body.message ?? "Credenciales inválidas.");
  }

  return res.json() as Promise<AuthSession>;
}

export async function logout(accessToken: string): Promise<void> {
  const res = await fetch(`${getApiBaseUrl()}/api/v1/auth/logout`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (!res.ok && res.status !== 204) {
    throw new Error("No se pudo cerrar la sesión.");
  }
}

export async function activateAccount(
  payload: ActivateAccountPayload,
): Promise<ActivateAccountResult> {
  const res = await fetch(`${getApiBaseUrl()}/api/v1/auth/users/activate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const body = (await res.json().catch(() => ({}))) as { message?: string };
    const message = body.message ?? "No se pudo activar la cuenta.";
    const error = new Error(message) as Error & { status?: number };
    error.status = res.status;
    throw error;
  }

  return res.json() as Promise<ActivateAccountResult>;
}
