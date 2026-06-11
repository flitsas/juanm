import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { InviteUserForm } from "./invite-user-form";

vi.mock("../api/admin-api", () => ({
  inviteUser: vi.fn(),
}));

import { inviteUser } from "../api/admin-api";

describe("InviteUserForm", () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it("muestra error cuando el correo ya existe en el tenant", async () => {
    vi.mocked(inviteUser).mockRejectedValue(
      new Error("El email ya está registrado en este tenant."),
    );

    render(
      <InviteUserForm
        accessToken="jwt"
        tenants={[{ tenantId: "t1", label: "Tenant Demo", users: [] }]}
        onInvited={vi.fn()}
      />,
    );

    fireEvent.change(screen.getByLabelText(/correo/i), {
      target: { value: "duplicado@example.com" },
    });
    fireEvent.click(screen.getByRole("button", { name: /invitar/i }));

    const alert = await screen.findByRole("alert");
    expect(alert.textContent).toMatch(/registrado/i);
  });

  it("confirma invitación con usuario pending", async () => {
    vi.mocked(inviteUser).mockResolvedValue({
      userId: "u1",
      tenantId: "t1",
      email: "invited@example.com",
      status: "Pending",
    });

    render(
      <InviteUserForm
        accessToken="jwt"
        tenants={[{ tenantId: "t1", label: "Tenant Demo", users: [] }]}
        onInvited={vi.fn()}
      />,
    );

    fireEvent.change(screen.getByLabelText(/correo/i), {
      target: { value: "invited@example.com" },
    });
    fireEvent.click(screen.getByRole("button", { name: /invitar/i }));

    await waitFor(() => {
      expect(inviteUser).toHaveBeenCalled();
      expect(screen.getByRole("status").textContent).toMatch(/pending/i);
    });
  });
});
