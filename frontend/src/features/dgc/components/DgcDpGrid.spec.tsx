import { screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { renderWithQueryClient } from "@/test/render-with-query-client";
import { DgcDpGrid } from "./DgcDpGrid";

vi.mock("@/features/plantillas/api/use-plantillas", () => ({
  useDerechosPeticionList: () => ({
    data: { items: [] },
    isLoading: false,
    isError: false,
  }),
}));

describe("DgcDpGrid", () => {
  it("muestra estado vacío sin error", () => {
    renderWithQueryClient(<DgcDpGrid comparendoId="cmp-1" />);
    expect(screen.getByTestId("gdc-dp-empty")).toBeTruthy();
    expect(screen.getByText("Sin derechos de petición")).toBeTruthy();
  });
});
