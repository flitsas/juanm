import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const push = vi.fn();
const searchParams = vi.hoisted(() => new URLSearchParams("token=abc123"));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
  useSearchParams: () => searchParams,
}));

vi.mock("../api/auth-api", () => ({
  activateAccount: vi.fn(),
}));

import { activateAccount } from "../api/auth-api";
import { ActivateAccountForm } from "./activate-account-form";

describe("ActivateAccountForm", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    searchParams.set("token", "abc123");
  });

  afterEach(() => {
    cleanup();
  });

  it("muestra error cuando falta el token en la URL", () => {
    searchParams.delete("token");

    render(<ActivateAccountForm />);

    const alert = screen.getByRole("alert");
    expect(alert.textContent).toMatch(/no es válido|falta el token/i);
    expect(activateAccount).not.toHaveBeenCalled();
  });

  it("activa cuenta con token válido", async () => {
    vi.mocked(activateAccount).mockResolvedValue({
      userId: "u1",
      email: "user@example.com",
      status: "Active",
    });

    render(<ActivateAccountForm />);

    fireEvent.change(screen.getByLabelText(/nueva contraseña/i), {
      target: { value: "SecurePass1!" },
    });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), {
      target: { value: "SecurePass1!" },
    });
    fireEvent.click(screen.getByRole("button", { name: /activar cuenta/i }));

    await waitFor(() => {
      expect(activateAccount).toHaveBeenCalledWith({
        token: "abc123",
        password: "SecurePass1!",
      });
      expect(push).toHaveBeenCalledWith("/login?activated=1");
    });
  });

  it("muestra error cuando el token expiró (HTTP 410)", async () => {
    vi.mocked(activateAccount).mockRejectedValue(
      Object.assign(new Error("El enlace de activación ha expirado."), { status: 410 }),
    );

    render(<ActivateAccountForm />);

    fireEvent.change(screen.getByLabelText(/nueva contraseña/i), {
      target: { value: "SecurePass1!" },
    });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), {
      target: { value: "SecurePass1!" },
    });
    fireEvent.click(screen.getByRole("button", { name: /activar cuenta/i }));

    const alert = await screen.findByRole("alert");
    expect(alert.textContent).toMatch(/expirado/i);
  });

  it("muestra error si las contraseñas no coinciden", async () => {
    render(<ActivateAccountForm />);

    fireEvent.change(screen.getByLabelText(/nueva contraseña/i), {
      target: { value: "SecurePass1!" },
    });
    fireEvent.change(screen.getByLabelText(/confirmar contraseña/i), {
      target: { value: "OtraPass1!" },
    });
    fireEvent.click(screen.getByRole("button", { name: /activar cuenta/i }));

    const alert = await screen.findByRole("alert");
    expect(alert.textContent).toMatch(/no coinciden/i);
    expect(activateAccount).not.toHaveBeenCalled();
  });
});
