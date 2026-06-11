import { describe, expect, it } from "vitest";
import { MAESTRA_COLUMNS } from "./comparendo-maestra-columns";

describe("MAESTRA_COLUMNS", () => {
  it("define las 15 columnas maestra DGC", () => {
    expect(MAESTRA_COLUMNS).toHaveLength(15);
    expect(MAESTRA_COLUMNS.map((c) => c.key)).toEqual([
      "estado",
      "numeroComparendo",
      "infractor",
      "documento",
      "placa",
      "infraccion",
      "fechaComparendo",
      "fechaNotificacion",
      "diasRestantes",
      "secretaria",
      "total",
      "pago",
      "contraventor",
      "dp",
      "fuente",
    ]);
  });
});
