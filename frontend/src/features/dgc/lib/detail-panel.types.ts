import type { ComparendoMaestraItem } from "./comparendo-maestra.types";

export type DetailPanelTab = "detalle" | "contraventor" | "correos";

export const DETAIL_PANEL_TABS: { id: DetailPanelTab; label: string }[] = [
  { id: "detalle", label: "Detalle" },
  { id: "contraventor", label: "Contraventor" },
  { id: "correos", label: "Log de correos" },
];

export type EmailLogItem = {
  id: string;
  sentAt: string;
  origen: string;
  destino: string;
  cc: string | null;
  tipoAlerta: string;
  estadoEntrega: string;
};

export type EmailLogListResult = {
  items: EmailLogItem[];
};

export type EmailEvidence = {
  id: string;
  htmlEvidencia: string;
};

export type ContraventorForm = {
  nombre: string;
  documento: string;
  correo: string;
};

export type ComparendoDetailState = {
  detail: ComparendoMaestraItem | null;
  emails: EmailLogItem[];
  loading: boolean;
  error: string | null;
};
