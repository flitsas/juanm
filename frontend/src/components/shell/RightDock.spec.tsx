import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { RightDock } from "./RightDock";

vi.mock("next/navigation", () => ({
  usePathname: () => "/dgc",
  useRouter: () => ({ push: vi.fn() }),
}));

vi.mock("next/image", () => ({
  default: (props: { alt?: string; src: string; className?: string }) => (
    // eslint-disable-next-line @next/next/no-img-element
    <img alt={props.alt ?? ""} src={props.src} className={props.className} />
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

describe("RightDock", () => {
  it("renderiza navegación con Comparendos activo en /dgc", () => {
    render(<RightDock />);
    expect(screen.getByTestId("flit-right-dock")).toBeTruthy();
    const comparendos = screen.getByTestId("dock-nav-dgc");
    expect(comparendos.getAttribute("aria-current")).toBe("page");
    expect(comparendos.getAttribute("href")).toBe("/dgc");
  });

  it("expone enlace al dashboard", () => {
    render(<RightDock />);
    const dashboard = screen.getByTestId("dock-nav-dashboard");
    expect(dashboard.getAttribute("href")).toBe("/");
  });
});
