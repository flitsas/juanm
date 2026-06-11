import { render, screen } from "@testing-library/react";
import { ThemeProvider } from "next-themes";
import { describe, expect, it, vi } from "vitest";
import { AppHeader } from "./AppHeader";

vi.mock("next/image", () => ({
  default: (props: { alt?: string; src: string }) => (
    // biome-ignore lint/performance/noImgElement: mock ligero de next/image en tests
    <img alt={props.alt ?? ""} src={props.src} />
  ),
}));

vi.mock("next/link", () => ({
  default: ({
    href,
    children,
    ...props
  }: {
    href: string;
    children: React.ReactNode;
    [key: string]: unknown;
  }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ replace: vi.fn() }),
}));

vi.mock("@/features/auth/lib/session", () => ({
  getSession: () => ({
    accessToken: "jwt-token",
    expiresAt: "2099-01-01T00:00:00Z",
    userId: "user-1",
    tenantId: "22222222-2222-2222-2222-222222222222",
    email: "admin@flit.dev",
    role: "SuperAdmin",
  }),
  clearSession: vi.fn(),
}));

function renderHeader() {
  return render(
    <ThemeProvider attribute="class" defaultTheme="light" enableSystem={false}>
      <AppHeader />
    </ThemeProvider>,
  );
}

describe("AppHeader", () => {
  it("muestra logo Flit Ready con enlace al dashboard", () => {
    renderHeader();
    const logoLink = screen.getByRole("link", { name: /Ir al dashboard/i });
    expect(logoLink.getAttribute("href")).toBe("/");
    expect(screen.getByAltText("Flit Ready")).toBeTruthy();
  });

  it("muestra toggle pill, badge de usuario y cerrar sesión", async () => {
    renderHeader();
    expect(screen.getByTestId("theme-toggle")).toBeTruthy();
    expect(screen.getByTestId("user-session-badge")).toBeTruthy();
    expect(screen.getByTestId("header-logout")).toBeTruthy();
    expect(screen.getByText("Admin")).toBeTruthy();
  });
});
