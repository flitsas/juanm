import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { LoginForm } from "./login-form";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push, replace: push }),
}));

vi.mock("../api/auth-api", () => ({
  login: vi.fn(),
}));

vi.mock("../lib/session", () => ({
  setSession: vi.fn(),
}));

import { login } from "../api/auth-api";
import { setSession } from "../lib/session";

describe("LoginForm", () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it("redirige al dashboard tras login exitoso", async () => {
    vi.mocked(login).mockResolvedValue({
      accessToken: "jwt",
      expiresAt: "2026-12-31T00:00:00Z",
      userId: "11111111-1111-4111-8111-111111111111",
      tenantId: "22222222-2222-4222-8222-222222222201",
      email: "user@example.com",
      role: "Operator",
    });

    render(<LoginForm />);

    fireEvent.change(screen.getByLabelText(/correo/i), {
      target: { value: "user@example.com" },
    });
    fireEvent.change(screen.getByLabelText(/contraseña/i), {
      target: { value: "Str0ng!Pass" },
    });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    await waitFor(() => {
      expect(setSession).toHaveBeenCalled();
      expect(push).toHaveBeenCalledWith("/dashboard");
    });
  });

  it("muestra mensaje accesible sin detalle por campo en error", async () => {
    vi.mocked(login).mockRejectedValue(new Error("Credenciales inválidas."));

    render(<LoginForm />);

    fireEvent.change(screen.getByLabelText(/correo/i), {
      target: { value: "user@example.com" },
    });
    fireEvent.change(screen.getByLabelText(/contraseña/i), {
      target: { value: "bad" },
    });
    fireEvent.click(screen.getByRole("button", { name: /ingresar/i }));

    const alert = await screen.findByRole("alert");
    expect(alert.textContent).toBe("Credenciales inválidas.");
  });
});
