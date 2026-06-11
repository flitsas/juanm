import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { ComparendoMaestraItem } from "../lib/comparendo-maestra.types";
import { MAESTRA_COLUMNS } from "../lib/comparendo-maestra-columns";
import { DgcMaestraTable } from "./DgcMaestraTable";

const item: ComparendoMaestraItem = {
  id: "a1",
  estado: "Pendiente",
  numeroComparendo: "CMP-100",
  infractor: "Maria",
  documento: "9988",
  placa: "XYZ999",
  infraccion: "C29",
  fechaComparendo: "2026-06-01",
  fechaNotificacion: "2026-06-05",
  diasRestantes: 10,
  secretaria: "sec-1",
  total: 250000,
  pago: "No",
  contraventor: "Maria (9988)",
  contraventorNombre: "Maria",
  contraventorDocumento: "9988",
  contraventorCorreo: null,
  dp: "DP-1",
  fuente: "ocr",
};

describe("DgcMaestraTable", () => {
  it("renderiza encabezados de 15 columnas", () => {
    render(<DgcMaestraTable items={[item]} />);
    const table = screen.getByTestId("dgc-maestra-table");
    for (const col of MAESTRA_COLUMNS) {
      expect(within(table).getByText(col.label)).toBeTruthy();
    }
  });

  it("renderiza fila de datos", () => {
    render(<DgcMaestraTable items={[item]} />);
    const table = screen.getByTestId("dgc-maestra-table");
    expect(within(table).getByText("CMP-100")).toBeTruthy();
    expect(within(table).getByText("Maria (9988)")).toBeTruthy();
  });
});
