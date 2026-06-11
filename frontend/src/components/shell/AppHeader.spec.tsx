import { render, screen } from "@testing-library/react";
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

describe("AppHeader", () => {
  it("muestra logo Flit Ready con enlace al dashboard", () => {
    render(<AppHeader />);
    const logoLink = screen.getByRole("link", { name: /Ir al dashboard/i });
    expect(logoLink.getAttribute("href")).toBe("/");
    expect(screen.getByAltText("Flit Ready")).toBeTruthy();
  });
});
