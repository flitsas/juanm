export const REGLAS_NODE_TYPES = {
  group: "group",
  predicate: "predicate",
} as const;

export const REGLAS_LOGIC_OPERATORS = {
  and: "and",
  or: "or",
} as const;

export const REGLAS_COMPARISON_OPERATORS = {
  eq: "eq",
  neq: "neq",
  gt: "gt",
  lt: "lt",
  contains: "contains",
  not_contains: "not_contains",
} as const;

export const REGLAS_FIELD_OPTIONS = [
  { label: "Estado", value: "estado" },
  { label: "Número comparendo", value: "numero_comparendo" },
  { label: "Placa", value: "placa" },
  { label: "Infractor", value: "infractor_nombre" },
  { label: "Fecha comparendo", value: "fecha_comparendo" },
  { label: "Fecha notificación", value: "fecha_notificacion" },
  { label: "Correo destino", value: "destino_email" },
] as const;

export const REGLAS_OPERATOR_OPTIONS = [
  { label: "Igual a", value: REGLAS_COMPARISON_OPERATORS.eq },
  { label: "Distinto de", value: REGLAS_COMPARISON_OPERATORS.neq },
  { label: "Mayor que", value: REGLAS_COMPARISON_OPERATORS.gt },
  { label: "Menor que", value: REGLAS_COMPARISON_OPERATORS.lt },
  { label: "Contiene", value: REGLAS_COMPARISON_OPERATORS.contains },
  { label: "No contiene", value: REGLAS_COMPARISON_OPERATORS.not_contains },
] as const;

export const REGLAS_LOGIC_OPTIONS = [
  { label: "Todas (Y)", value: REGLAS_LOGIC_OPERATORS.and },
  { label: "Alguna (O)", value: REGLAS_LOGIC_OPERATORS.or },
] as const;
