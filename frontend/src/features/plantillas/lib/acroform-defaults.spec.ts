import { describe, expect, it } from "vitest";
import { normalizeTemplateField, resolveFieldType } from "./acroform-defaults";
import type { PdfTemplateField } from "./plantillas.types";

describe("acroform-defaults", () => {
  it("resuelve comparendo_estado como choice aunque el PDF lo detecte text", () => {
    expect(resolveFieldType("comparendo_estado", "text")).toBe("choice");
    expect(resolveFieldType("comparendo_total", "text")).toBe("number");
  });

  it("normaliza campo con variable y opciones por defecto", () => {
    const field: PdfTemplateField = {
      id: "f1",
      acroformName: "comparendo_estado",
      fieldType: "text",
      systemVariable: null,
      choiceOptions: null,
      sortOrder: 0,
    };

    const normalized = normalizeTemplateField(field);
    expect(normalized.fieldType).toBe("choice");
    expect(normalized.systemVariable).toBe("comparendo.estado");
    expect(normalized.choiceOptions).toEqual([
      "Pendiente",
      "Notificado",
      "En trámite",
      "Pagado",
      "Cerrado",
    ]);
  });
});
