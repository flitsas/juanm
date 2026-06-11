import { afterEach, describe, expect, it } from "vitest";
import { clearSession, getSession, setSession } from "./session";

const sampleSession = {
  accessToken: "jwt-token",
  expiresAt: "2026-12-31T00:00:00Z",
  userId: "11111111-1111-4111-8111-111111111111",
  tenantId: "22222222-2222-4222-8222-222222222201",
  email: "user@example.com",
  role: "Operator",
};

describe("session", () => {
  afterEach(() => {
    clearSession();
  });

  it("almacena y recupera la sesión", () => {
    setSession(sampleSession);
    expect(getSession()).toEqual(sampleSession);
  });

  it("limpia la sesión al cerrar", () => {
    setSession(sampleSession);
    clearSession();
    expect(getSession()).toBeNull();
  });
});
