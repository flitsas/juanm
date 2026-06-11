import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { RuleFormDialog } from "./RuleFormDialog";

const templates = [
  {
    id: "tpl-1",
    name: "Notificación inicial",
    subject: "S",
    htmlBody: "<p>x</p>",
    bannerUrl: null,
    footerUrl: null,
    createdAt: "2026-06-01",
    updatedAt: null,
  },
];

describe("RuleFormDialog", () => {
  it("muestra validación si falta plantilla al guardar", () => {
    render(
      <RuleFormDialog open rule={null} templates={[]} onOpenChange={vi.fn()} onSubmit={vi.fn()} />,
    );
    fireEvent.change(screen.getByLabelText("Nombre"), { target: { value: "R1" } });
    fireEvent.click(screen.getByTestId("rule-form-submit"));
    expect(screen.getByText("Seleccione una plantilla.")).toBeTruthy();
  });
});
