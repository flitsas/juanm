import { describe, expect, it } from "vitest";
import type { ComparendoMaestraItem } from "./comparendo-maestra.types";
import { formatMaestraCell } from "./format-maestra-cell";

const sample: ComparendoMaestraItem = {
  id: "1",
  estado: "Pendiente",
  numeroComparendo: "CMP-1",
  infractor: "Juan",
  documento: "123",
  placa: "ABC123",
  infraccion: "C29",
  fechaComparendo: "2026-05-10",
  fechaNotificacion: null,
  diasRestantes: 5,
  secretaria: null,
  total: 500000,
  pago: null,
  contraventor: "Pendiente",
  contraventorNombre: null,
  contraventorDocumento: null,
  contraventorCorreo: null,
  dp: null,
  fuente: "ocr",
};

describe("formatMaestraCell", () => {
  it("formatea fecha y moneda", () => {
    expect(formatMaestraCell(sample, "fechaComparendo")).toBe("10/05/2026");
    expect(formatMaestraCell(sample, "total")).toContain("500");
  });

  it("retorna guión para valores nulos", () => {
    expect(formatMaestraCell(sample, "fechaNotificacion")).toBe("—");
  });
});
