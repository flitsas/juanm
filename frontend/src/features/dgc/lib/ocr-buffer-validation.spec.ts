import { describe, expect, it } from "vitest";
import type { OcrBufferForm } from "./ocr.types";
import { hasOcrBufferErrors, validateOcrBufferForm } from "./ocr-buffer-validation";

const baseForm: OcrBufferForm = {
  itemId: "item-1",
  fileName: "comparendo.png",
  previewUrl: null,
  ocrDiagnostic: "",
  numeroComparendo: "CMP-001",
  estado: "Pendiente",
  infractorNombre: "Juan",
  documento: "123",
  placa: "ABC123",
  infraccionCodigo: "C29",
  fechaComparendo: "2026-06-01",
  totalValor: "150000",
  confidence: 0.9,
};

describe("validateOcrBufferForm", () => {
  it("no devuelve errores cuando los obligatorios están completos", () => {
    const errors = validateOcrBufferForm(baseForm);
    expect(hasOcrBufferErrors(errors)).toBe(false);
  });

  it("marca numeroComparendo obligatorio vacío", () => {
    const errors = validateOcrBufferForm({ ...baseForm, numeroComparendo: "   " });
    expect(errors.numeroComparendo).toBeTruthy();
    expect(hasOcrBufferErrors(errors)).toBe(true);
  });

  it("marca estado obligatorio vacío", () => {
    const errors = validateOcrBufferForm({ ...baseForm, estado: "" });
    expect(errors.estado).toBeTruthy();
  });
});
