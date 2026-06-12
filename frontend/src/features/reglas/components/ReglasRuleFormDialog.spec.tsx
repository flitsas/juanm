import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ReglasRuleFormDialog } from "./ReglasRuleFormDialog";

vi.mock("@/features/plantillas/components/PdfTemplateDropdown", () => ({
  PdfTemplateDropdown: ({
    value,
    onChange,
  }: {
    value: string;
    onChange: (id: string) => void;
  }) => (
    <select
      data-testid="reglas-pdf-template-dropdown"
      value={value}
      onChange={(e) => onChange(e.target.value)}
    >
      <option value="">Seleccione</option>
      <option value="33333333-3333-3333-3333-333333333333">DP Vialix</option>
    </select>
  ),
}));

vi.mock("@/features/plantillas/api/use-plantillas", () => ({
  usePdfTemplates: () => ({
    data: {
      items: [
        {
          id: "33333333-3333-3333-3333-333333333333",
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
}));

function renderDialog(open = true) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <ReglasRuleFormDialog
        open={open}
        rule={null}
        contacts={[]}
        onOpenChange={vi.fn()}
        onSubmit={vi.fn()}
      />
    </QueryClientProvider>,
  );
}

describe("ReglasRuleFormDialog", () => {
  it("muestra selector de plantilla PDF en lugar de UUID manual", () => {
    renderDialog();
    expect(screen.getByTestId("reglas-pdf-template-dropdown")).toBeTruthy();
  });

  it("preselecciona plantilla activa al crear regla", () => {
    renderDialog();
    const select = screen.getByTestId(
      "reglas-pdf-template-dropdown",
    ) as HTMLSelectElement;
    expect(select.value).toBe("33333333-3333-3333-3333-333333333333");
  });
});
