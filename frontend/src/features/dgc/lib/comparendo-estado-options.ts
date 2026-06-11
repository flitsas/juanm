export const COMPARENDO_ESTADO_OPTIONS = [
  { label: "Pendiente", value: "Pendiente" },
  { label: "Impugnado", value: "Impugnado" },
  { label: "Pagado", value: "Pagado" },
  { label: "Prescrito", value: "Prescrito" },
] as const;

export const COMPARENDO_ESTADO_FILTER_OPTIONS = [
  { label: "Todos", value: "" },
  ...COMPARENDO_ESTADO_OPTIONS,
];
