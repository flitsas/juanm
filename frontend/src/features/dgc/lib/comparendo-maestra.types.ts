export type ComparendoMaestraItem = {
  id: string;
  estado: string;
  numeroComparendo: string;
  infractor: string | null;
  documento: string | null;
  placa: string | null;
  infraccion: string | null;
  fechaComparendo: string | null;
  fechaNotificacion: string | null;
  diasRestantes: number | null;
  secretaria: string | null;
  total: number;
  pago: string | null;
  contraventor: string | null;
  contraventorNombre: string | null;
  contraventorDocumento: string | null;
  contraventorCorreo: string | null;
  dp: string | null;
  fuente: string;
};

export type ComparendoMaestraListResult = {
  items: ComparendoMaestraItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type ComparendoMaestraFilters = {
  search: string;
  estado: string;
  secretaria: string;
  fechaDesde: string;
  fechaHasta: string;
  page: number;
  pageSize: number;
};

export const DEFAULT_FILTERS: ComparendoMaestraFilters = {
  search: "",
  estado: "",
  secretaria: "",
  fechaDesde: "",
  fechaHasta: "",
  page: 1,
  pageSize: 20,
};
