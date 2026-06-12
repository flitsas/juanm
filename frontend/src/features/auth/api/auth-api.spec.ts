import { afterEach, describe, expect, it, vi } from "vitest";
import { login, logout } from "./auth-api";

describe("auth-api", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("login retorna sesión cuando el API responde 200", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          accessToken: "token",
          expiresAt: "2026-12-31T00:00:00Z",
          userId: "11111111-1111-4111-8111-111111111111",
          tenantId: "22222222-2222-2222-2222-222222222222",
          email: "user@example.com",
          role: "Operator",
        }),
      }),
    );

    const session = await login({ email: "user@example.com", password: "secret" });
    expect(session.accessToken).toBe("token");
  });

  it("login lanza error genérico en 401", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: false,
        status: 401,
        json: async () => ({ message: "Credenciales inválidas." }),
      }),
    );

    await expect(login({ email: "x@y.com", password: "bad" })).rejects.toThrow(
      "Credenciales inválidas.",
    );
  });

  it("logout envía bearer token", async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, status: 204 });
    vi.stubGlobal("fetch", fetchMock);

    await logout("jwt-abc");
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining("/api/v1/auth/logout"),
      expect.objectContaining({
        method: "POST",
        headers: expect.objectContaining({
          Authorization: "Bearer jwt-abc",
        }),
      }),
    );
  });
});
