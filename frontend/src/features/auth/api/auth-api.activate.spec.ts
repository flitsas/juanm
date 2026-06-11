import { describe, expect, it, vi } from "vitest";
import { activateAccount } from "./auth-api";

describe("auth-api activateAccount", () => {
  it("envía token y contraseña al endpoint de activación", async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ userId: "u1", email: "a@b.co", status: "Active" }),
    });
    vi.stubGlobal("fetch", fetchMock);

    const result = await activateAccount({ token: "tok", password: "Pass1234!" });

    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining("/auth/users/activate"),
      expect.objectContaining({
        method: "POST",
        body: JSON.stringify({ token: "tok", password: "Pass1234!" }),
      }),
    );
    expect(result.status).toBe("Active");
  });

  it("propaga status 410 en token expirado", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: false,
        status: 410,
        json: async () => ({ message: "El token de activación ha expirado." }),
      }),
    );

    await expect(activateAccount({ token: "old", password: "Pass1234!" })).rejects.toMatchObject({
      status: 410,
    });
  });
});
