import { fireEvent, render, screen } from "@testing-library/react";
import { ThemeProvider } from "next-themes";
import { describe, expect, it } from "vitest";
import { ThemeToggle } from "./theme-toggle";

describe("ThemeToggle", () => {
  it("alterna el pill del header entre modo claro y oscuro", async () => {
    render(
      <ThemeProvider attribute="class" defaultTheme="light" enableSystem={false}>
        <ThemeToggle variant="pill" />
      </ThemeProvider>,
    );

    const toggle = await screen.findByTestId("theme-toggle");
    expect(toggle.getAttribute("aria-label")).toBe("Cambiar a modo oscuro");

    fireEvent.click(toggle);
    expect(toggle.getAttribute("aria-label")).toBe("Cambiar a modo claro");
  });

  it("alterna el botón icono del login", async () => {
    render(
      <ThemeProvider attribute="class" defaultTheme="dark" enableSystem={false}>
        <ThemeToggle variant="icon" />
      </ThemeProvider>,
    );

    const toggle = await screen.findByTestId("theme-toggle");
    expect(toggle.getAttribute("aria-label")).toBe("Activar modo claro");
  });
});
