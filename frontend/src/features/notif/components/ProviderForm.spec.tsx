import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ProviderForm } from "./ProviderForm";

function renderWithQuery(ui: React.ReactElement) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={client}>{ui}</QueryClientProvider>);
}

describe("ProviderForm", () => {
  it("muestra validación inline al guardar sin datos", async () => {
    renderWithQuery(<ProviderForm provider={null} />);
    fireEvent.click(screen.getByTestId("provider-save-btn"));
    expect(await screen.findByText("Seleccione un proveedor.")).toBeTruthy();
    expect(screen.getByText("El remitente es obligatorio.")).toBeTruthy();
  });
});
