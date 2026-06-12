import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { GdcPlantillasPage } from "./GdcPlantillasPage";

vi.mock("./TemplateGenerationDialog", () => ({
  TemplateGenerationDialog: () => <div data-testid="gdc-generation-dialog-stub" />,
}));

vi.mock("../api/use-plantillas", () => ({
  usePdfTemplates: () => ({
    data: {
      items: [
        {
          id: "tpl-1",
          name: "DP Vialix",
          version: 1,
          isActive: true,
          fieldCount: 13,
        },
      ],
    },
    isLoading: false,
    isError: false,
  }),
  usePdfTemplate: () => ({ data: null, isLoading: false }),
  useSystemVariables: () => ({
    data: {
      items: [
        { key: "comparendo.numero", label: "Número", source: "comparendo", dataType: "text" },
      ],
    },
  }),
  usePlantillaMutations: () => ({
    upload: { mutateAsync: vi.fn(), isPending: false },
    saveMappings: { mutateAsync: vi.fn(), isPending: false },
    activate: { mutateAsync: vi.fn(), isPending: false },
  }),
}));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={client}>
      <GdcPlantillasPage />
    </QueryClientProvider>,
  );
}

describe("GdcPlantillasPage", () => {
  it("muestra catálogo con acción Usar plantilla", () => {
    renderPage();
    expect(screen.getByTestId("gdc-templates-catalog")).toBeTruthy();
    expect(screen.getByText("Usar plantilla")).toBeTruthy();
    expect(screen.getByText("DP Vialix")).toBeTruthy();
  });

  it("muestra botón Nueva plantilla sin icono en label", () => {
    renderPage();
    const btn = screen.getByTestId("gdc-new-template-btn");
    expect(btn.textContent).toBe("Nueva plantilla");
  });
});
