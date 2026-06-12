import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { TemplateGenerationDialog } from "./TemplateGenerationDialog";

vi.mock("@/features/dgc/api/use-comparendos-maestra", () => ({
  useComparendosMaestra: () => ({
    data: {
      items: [
        {
          id: "cmp-blocked",
          estado: "Pendiente",
          numeroComparendo: "CMP-BLOCK",
          infractor: null,
          documento: null,
          placa: "XYZ999",
          infraccion: null,
          fechaComparendo: null,
          fechaNotificacion: null,
          diasRestantes: null,
          secretaria: null,
          total: 0,
          pago: null,
          contraventor: null,
          contraventorNombre: null,
          contraventorDocumento: null,
          contraventorCorreo: null,
          dp: null,
          fuente: "ocr",
        },
      ],
    },
    isLoading: false,
    isError: false,
  }),
}));

vi.mock("../api/use-dp-generation", () => ({
  useDpGeneration: () => ({
    generate: { mutateAsync: vi.fn(), isPending: false },
    download: { mutateAsync: vi.fn(), isPending: false },
  }),
}));

const template = {
  id: "tpl-1",
  name: "DP Vialix",
  version: 1,
  isActive: true,
  fieldCount: 13,
};

function renderDialog() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={client}>
      <TemplateGenerationDialog template={template} visible onHide={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe("TemplateGenerationDialog", () => {
  it("muestra selector de comparendo", () => {
    renderDialog();
    expect(screen.getByTestId("gdc-comparendo-select")).toBeTruthy();
    expect(screen.getByTestId("gdc-generate-preview-btn")).toBeTruthy();
  });

  it("deshabilita descarga sin preview generada", () => {
    renderDialog();
    const downloadBtn = screen.getByTestId("gdc-download-pdf-btn");
    expect(downloadBtn.hasAttribute("disabled")).toBe(true);
  });
});
