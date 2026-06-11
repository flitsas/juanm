import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { EmptyState, ErrorState, LoadingState } from "./ui-state";

describe("ui-state", () => {
  it("LoadingState expone estado de carga accesible", () => {
    render(<LoadingState label="Cargando usuarios" />);
    expect(screen.getByRole("status").textContent).toBe("Cargando usuarios");
  });

  it("EmptyState muestra mensaje vacío", () => {
    render(<EmptyState title="Sin usuarios" description="Invita el primero." />);
    expect(screen.getByText("Sin usuarios")).toBeTruthy();
  });

  it("ErrorState usa role alert", () => {
    render(<ErrorState message="No se pudo cargar la consola." />);
    expect(screen.getByRole("alert").textContent).toBe("No se pudo cargar la consola.");
  });
});
