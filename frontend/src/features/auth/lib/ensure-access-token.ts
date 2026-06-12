import { getApiBaseUrl } from "@/lib/api-base-url";
import type { AuthSession } from "../types";
import { clearSession, getSession, setSession } from "./session";

const REFRESH_SKEW_MS = 60_000;
let refreshPromise: Promise<AuthSession | null> | null = null;

export class SessionExpiredError extends Error {
  constructor(message = "Sesión expirada. Inicie sesión nuevamente.") {
    super(message);
    this.name = "SessionExpiredError";
  }
}

function isAccessTokenFresh(session: AuthSession): boolean {
  const expiresAt = Date.parse(session.expiresAt);
  if (!Number.isFinite(expiresAt)) {
    return false;
  }

  return expiresAt - Date.now() > REFRESH_SKEW_MS;
}

export async function refreshSession(refreshToken: string): Promise<AuthSession> {
  const res = await fetch(`${getApiBaseUrl()}/api/v1/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  });

  if (!res.ok) {
    const body = (await res.json().catch(() => ({}))) as { message?: string };
    throw new SessionExpiredError(body.message ?? "Sesión expirada. Inicie sesión nuevamente.");
  }

  return res.json() as Promise<AuthSession>;
}

export async function ensureAccessToken(): Promise<string> {
  const session = getSession();
  if (!session?.accessToken) {
    throw new SessionExpiredError();
  }

  if (isAccessTokenFresh(session)) {
    return session.accessToken;
  }

  if (!session.refreshToken) {
    clearSession();
    throw new SessionExpiredError();
  }

  if (!refreshPromise) {
    refreshPromise = refreshSession(session.refreshToken)
      .then((refreshed) => {
        setSession(refreshed);
        return refreshed;
      })
      .catch((error) => {
        clearSession();
        throw error;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  const refreshed = await refreshPromise;
  if (!refreshed?.accessToken) {
    throw new SessionExpiredError();
  }

  return refreshed.accessToken;
}
