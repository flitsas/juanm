import { describe, expect, it } from "vitest";
import type { ComparendoMaestraItem } from "@/features/dgc/lib/comparendo-maestra.types";
import { hasContraventorIdentificado } from "./comparendo-eligibility";

const baseComparendo: ComparendoMaestraItem = {
  id: "cmp-1",
  estado: "Notificado",
  numeroComparendo: "CMP-001",
  infractor: "Juan",
  documento: "123",
  placa: "ABC123",
  infraccion: null,
  fechaComparendo: "2026-05-15",
  fechaNotificacion: null,
  diasRestantes: null,
  secretaria: null,
  total: 100,
  pago: null,
  contraventor: null,
  contraventorNombre: "Maria Lopez",
  contraventorDocumento: "55443322",
  contraventorCorreo: "maria@flit.test",
  dp: null,
  fuente: "ocr",
};

describe("hasContraventorIdentificado", () => {
  it("acepta comparendo con contraventor nombre y documento", () => {
    expect(hasContraventorIdentificado(baseComparendo)).toBe(true);
  });

  it("rechaza comparendo sin contraventor", () => {
    expect(
      hasContraventorIdentificado({
        ...baseComparendo,
        contraventorNombre: null,
        contraventorDocumento: null,
        contraventor: null,
      }),
    ).toBe(false);
  });
});
