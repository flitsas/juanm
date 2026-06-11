import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { RuleFormDialog } from "./RuleFormDialog";

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
