import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { LogoutButton } from "./logout-button";

const replace = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace, push: replace }),
}));

vi.mock("../api/auth-api", () => ({
  logout: vi.fn().mockResolvedValue(undefined),
}));

vi.mock("../lib/session", () => ({
  getSession: vi.fn(() => ({ accessToken: "jwt-token" })),
  clearSession: vi.fn(),
}));

import { logout } from "../api/auth-api";
import { clearSession } from "../lib/session";

describe("LogoutButton", () => {
  it("cierra sesión y redirige al login", async () => {
    render(<LogoutButton />);
    fireEvent.click(screen.getByRole("button", { name: /cerrar sesión/i }));

    await waitFor(() => {
      expect(logout).toHaveBeenCalledWith("jwt-token");
      expect(clearSession).toHaveBeenCalled();
      expect(replace).toHaveBeenCalledWith("/login");
    });
  });
});
