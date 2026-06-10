import { afterEach, describe, expect, it, vi } from "vitest";
import { getRbacMatrix, inviteUser, listAdminUsers, updateRbacMatrix } from "./admin-api";

describe("admin-api", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("listAdminUsers envía bearer y retorna usuarios", async () => {
    const users = [
      {
        id: "11111111-1111-4111-8111-111111111111",
        tenantId: "22222222-2222-4222-8222-222222222201",
        email: "a@example.com",
        status: "Active",
      },
    ];
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => users,
      }),
    );

    const result = await listAdminUsers("jwt-super");
    expect(result).toEqual(users);
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining("/auth/admin/users"),
      expect.objectContaining({
        headers: expect.objectContaining({ Authorization: "Bearer jwt-super" }),
      }),
    );
  });

  it("inviteUser crea usuario pending", async () => {
    const created = {
      userId: "33333333-3333-4333-8333-333333333333",
      tenantId: "22222222-2222-4222-8222-222222222201",
      email: "new@example.com",
      status: "Pending",
    };
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => created,
      }),
    );

    const result = await inviteUser("jwt-super", {
      email: "new@example.com",
      tenantId: "22222222-2222-4222-8222-222222222201",
    });
    expect(result.status).toBe("Pending");
  });

  it("getRbacMatrix retorna roles y permisos", async () => {
    const matrix = {
      permissions: [{ id: "p1", code: "users.read", module: "users", action: "read" }],
      roles: [{ roleId: "r1", code: "Operator", name: "Operador", permissionIds: [] }],
    };
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => matrix,
      }),
    );

    const result = await getRbacMatrix("jwt-super");
    expect(result.roles).toHaveLength(1);
  });

  it("updateRbacMatrix envía assignments", async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, status: 204 });
    vi.stubGlobal("fetch", fetchMock);

    await updateRbacMatrix("jwt-super", [
      { roleId: "r1", permissionId: "p1", enabled: true },
    ]);

    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining("/auth/rbac/matrix"),
      expect.objectContaining({
        method: "PUT",
        body: JSON.stringify({
          assignments: [{ roleId: "r1", permissionId: "p1", enabled: true }],
        }),
      }),
    );
  });
});
