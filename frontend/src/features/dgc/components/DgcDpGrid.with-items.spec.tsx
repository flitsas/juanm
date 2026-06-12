import { screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { renderWithQueryClient } from "@/test/render-with-query-client";
import { DgcDpGrid } from "./DgcDpGrid";

vi.mock("@/features/plantillas/api/use-plantillas", () => ({
  useDerechosPeticionList: () => ({
    data: {
      items: [
        {
          id: "dp-1",
          comparendoId: "cmp-1",
          templateId: "tpl-1",
          templateVersion: 2,
          estado: "Enviado",
          generatedAt: "2026-06-12T10:00:00Z",
          downloadPath: "/api/v1/gdc/derechos-peticion/dp-1/download",
        },
      ],
    },
    isLoading: false,
    isError: false,
  }),
}));

vi.mock("@/features/plantillas/api/plantillas-api", () => ({
  downloadDerechoPeticionPdf: vi.fn(),
}));

describe("DgcDpGrid con items", () => {
  it("renderiza grilla con badge de estado y descarga", () => {
    renderWithQueryClient(<DgcDpGrid comparendoId="cmp-1" />);
    expect(screen.getByTestId("gdc-dp-grid")).toBeTruthy();
    expect(screen.getByTestId("gdc-dp-estado-dp-1").textContent).toContain("Enviado");
    expect(screen.getByTestId("gdc-dp-download-dp-1")).toBeTruthy();
  });
});
