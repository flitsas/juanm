import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { UnifiedAdminPage } from "./unified-admin-page";

const replace = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace }),
  useSearchParams: () => new URLSearchParams("tab=notificaciones"),
}));

vi.mock("../lib/session", () => ({
  getSession: () => ({
    accessToken: "jwt",
    refreshToken: "refresh-token",
    role: "SuperAdmin",
    email: "admin@flit.dev",
    userId: "11111111-1111-4111-8111-111111111111",
    tenantId: "22222222-2222-2222-2222-222222222222",
    expiresAt: "2026-12-31T00:00:00Z",
  }),
}));

vi.mock("./admin-users-section", () => ({
  AdminUsersSection: () => <div data-testid="admin-users-section" />,
}));

vi.mock("./rbac-matrix-panel", () => ({
  RbacMatrixPanel: () => <div data-testid="rbac-matrix-panel" />,
}));

vi.mock("@/features/notif/components/NotifAdminPanel", () => ({
  NotifAdminPanel: () => <div data-testid="notif-admin-panel" />,
}));

describe("UnifiedAdminPage", () => {
  it("renderiza encabezado y pestaña de notificaciones desde query", () => {
    render(<UnifiedAdminPage />);

    expect(screen.getByTestId("unified-admin-page")).toBeTruthy();
    expect(screen.getByRole("heading", { name: "Administración" })).toBeTruthy();
    expect(screen.getByText("Multi-compañía, usuarios y roles")).toBeTruthy();
    expect(screen.getByTestId("notif-admin-panel")).toBeTruthy();
  });

  it("muestra pestañas de usuarios y RBAC para Super Admin", () => {
    render(<UnifiedAdminPage />);

    expect(screen.getByRole("button", { name: "Usuarios y compañías" })).toBeTruthy();
    expect(screen.getByRole("button", { name: "Matriz RBAC" })).toBeTruthy();
    expect(screen.getByRole("button", { name: "Notificaciones" })).toBeTruthy();
  });
});
