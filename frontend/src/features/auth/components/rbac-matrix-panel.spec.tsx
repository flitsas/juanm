import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { RbacMatrixPanel } from "./rbac-matrix-panel";

vi.mock("../api/admin-api", () => ({
  getRbacMatrix: vi.fn(),
  updateRbacMatrix: vi.fn(),
}));

import { getRbacMatrix, updateRbacMatrix } from "../api/admin-api";

const matrix = {
  permissions: [
    { id: "p1", code: "users.read", module: "users", action: "read", description: null },
  ],
  roles: [{ roleId: "r1", code: "Operator", name: "Operador", permissionIds: [] }],
};

describe("RbacMatrixPanel", () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it("guarda toggle y confirma estado actualizado", async () => {
    vi.mocked(getRbacMatrix).mockResolvedValue(matrix);
    vi.mocked(updateRbacMatrix).mockResolvedValue(undefined);

    render(<RbacMatrixPanel accessToken="jwt" />);

    const checkbox = await screen.findByRole("checkbox", {
      name: /users\.read.*operador/i,
    });
    fireEvent.click(checkbox);
    fireEvent.click(screen.getByRole("button", { name: /guardar cambios/i }));

    await waitFor(() => {
      expect(updateRbacMatrix).toHaveBeenCalled();
      expect(screen.getByRole("status").textContent).toMatch(/actualizad/i);
    });
  });
});
