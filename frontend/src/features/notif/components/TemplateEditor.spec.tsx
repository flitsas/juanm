import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { TemplateEditor } from "./TemplateEditor";

function renderEditor() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={client}>
      <TemplateEditor template={null} onCancel={vi.fn()} onSaved={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe("TemplateEditor", () => {
  it("muestra validación inline al guardar sin cuerpo", () => {
    renderEditor();
    fireEvent.change(screen.getByLabelText("Nombre"), { target: { value: "T1" } });
    fireEvent.change(screen.getByLabelText("Asunto"), { target: { value: "Asunto" } });
    fireEvent.click(screen.getByTestId("template-save-btn"));
    expect(screen.getByText("El cuerpo del mensaje es obligatorio.")).toBeTruthy();
  });

  it("muestra preview con banner en vivo", () => {
    renderEditor();
    fireEvent.change(screen.getByLabelText("Cuerpo HTML"), {
      target: { value: "<p>Contenido</p>" },
    });
    const preview = screen.getByTestId("notif-template-preview");
    expect(preview.textContent).toContain("Contenido");
  });
});
