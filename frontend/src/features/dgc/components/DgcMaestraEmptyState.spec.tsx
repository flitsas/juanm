import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { DgcMaestraEmptyState } from "./DgcMaestraEmptyState";

describe("DgcMaestraEmptyState", () => {
  it("muestra mensaje accionable sin comparendos", () => {
    render(<DgcMaestraEmptyState />);
    expect(screen.getByTestId("dgc-maestra-empty")).toBeTruthy();
    expect(screen.getByText(/Sin comparendos/i)).toBeTruthy();
    expect(screen.getByText(/Carga comparendos vía OCR/i)).toBeTruthy();
  });

  it("muestra botón limpiar filtros cuando aplica", () => {
    const onClear = vi.fn();
    render(<DgcMaestraEmptyState onClearFilters={onClear} />);
    expect(screen.getByRole("button", { name: /Limpiar filtros/i })).toBeTruthy();
  });
});
