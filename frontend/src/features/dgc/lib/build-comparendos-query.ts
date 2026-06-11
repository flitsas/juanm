import type { ComparendoMaestraFilters } from "./comparendo-maestra.types";

export function buildComparendosQuery(filters: ComparendoMaestraFilters): string {
  const params = new URLSearchParams();
  params.set("page", String(filters.page));
  params.set("pageSize", String(filters.pageSize));

  if (filters.search.trim()) {
    params.set("search", filters.search.trim());
  }
  if (filters.estado) {
    params.set("estado", filters.estado);
  }
  if (filters.secretaria) {
    params.set("secretaria", filters.secretaria);
  }
  if (filters.fechaDesde) {
    params.set("fechaDesde", filters.fechaDesde);
  }
  if (filters.fechaHasta) {
    params.set("fechaHasta", filters.fechaHasta);
  }

  return params.toString();
}
