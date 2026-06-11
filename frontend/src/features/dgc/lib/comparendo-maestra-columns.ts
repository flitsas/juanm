export type MaestraColumn = {
  key: keyof ComparendoMaestraColumnKeys;
  label: string;
};

export type ComparendoMaestraColumnKeys = {
  estado: string;
  numeroComparendo: string;
  infractor: string;
  documento: string;
  placa: string;
  infraccion: string;
  fechaComparendo: string;
  fechaNotificacion: string;
  diasRestantes: string;
  secretaria: string;
  total: string;
  pago: string;
  contraventor: string;
  dp: string;
  fuente: string;
};

/** 15 columnas maestra DGC (RF04) */
export const MAESTRA_COLUMNS: MaestraColumn[] = [
  { key: "estado", label: "Estado" },
  { key: "numeroComparendo", label: "No. Comparendo" },
  { key: "infractor", label: "Infractor" },
  { key: "documento", label: "Nro Documento" },
  { key: "placa", label: "Placa" },
  { key: "infraccion", label: "Infracción" },
  { key: "fechaComparendo", label: "Fecha Comparendo" },
  { key: "fechaNotificacion", label: "Fecha Notificación" },
  { key: "diasRestantes", label: "Días Restantes" },
  { key: "secretaria", label: "Secretaría" },
  { key: "total", label: "Total" },
  { key: "pago", label: "Pago" },
  { key: "contraventor", label: "Contraventor" },
  { key: "dp", label: "DP" },
  { key: "fuente", label: "Fuente" },
];
