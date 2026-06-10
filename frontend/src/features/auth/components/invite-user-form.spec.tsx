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
    fireEvent.change(screen.getByLabelText(/tenant/i), { target: { value: "t1" } });
    fireEvent.click(screen.getByRole("button", { name: /invitar/i }));

    await waitFor(() => {
      expect(inviteUser).toHaveBeenCalled();
      expect(screen.getByRole("status").textContent).toMatch(/pending/i);
    });
  });
});
