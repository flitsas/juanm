import { describe, expect, it } from "vitest";
import { buildComparendosQuery } from "./build-comparendos-query";
import { DEFAULT_FILTERS } from "./comparendo-maestra.types";

describe("buildComparendosQuery", () => {
  it("incluye paginación y filtros activos", () => {
    const query = buildComparendosQuery({
      ...DEFAULT_FILTERS,
      search: "ABC123",
      estado: "Pendiente",
      secretaria: "33333333-3333-3333-3333-333333333333",
      fechaDesde: "2026-01-01",
      fechaHasta: "2026-06-30",
      page: 2,
      pageSize: 10,
    });

    const params = new URLSearchParams(query);
    expect(params.get("page")).toBe("2");
    expect(params.get("pageSize")).toBe("10");
    expect(params.get("search")).toBe("ABC123");
    expect(params.get("estado")).toBe("Pendiente");
    expect(params.get("secretaria")).toBe("33333333-3333-3333-3333-333333333333");
    expect(params.get("fechaDesde")).toBe("2026-01-01");
    expect(params.get("fechaHasta")).toBe("2026-06-30");
  });
});
