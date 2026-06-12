import type { PdfTemplateField, PlantillaFieldType } from "./plantillas.types";

const FIELD_TYPE_OVERRIDES: Record<string, PlantillaFieldType> = {
  comparendo_total: "number",
  comparendo_estado: "choice",
};

const SYSTEM_VARIABLE_BY_ACROFORM: Record<string, string> = {
  tenant_nombre: "tenant.nombre",
  comparendo_numero: "comparendo.numero",
  comparendo_placa: "comparendo.placa",
  comparendo_documento: "comparendo.documento",
  comparendo_infractor_nombre: "comparendo.infractor_nombre",
  comparendo_fecha_comparendo: "comparendo.fecha_comparendo",
  comparendo_fecha_notificacion: "comparendo.fecha_notificacion",
  comparendo_total: "comparendo.total",
  comparendo_estado: "comparendo.estado",
  contraventor_nombre: "contraventor.nombre",
  contraventor_documento: "contraventor.documento",
  contraventor_correo: "contraventor.correo",
  secretaria_destino: "derecho_peticion.secretaria_destino",
};

const CHOICE_OPTIONS_BY_ACROFORM: Record<string, string[]> = {
  comparendo_estado: ["Pendiente", "Notificado", "En trámite", "Pagado", "Cerrado"],
};

export function resolveFieldType(
  acroformName: string,
  detectedType: PlantillaFieldType,
): PlantillaFieldType {
  return FIELD_TYPE_OVERRIDES[acroformName] ?? detectedType;
}

export function suggestSystemVariable(acroformName: string): string | null {
  return SYSTEM_VARIABLE_BY_ACROFORM[acroformName] ?? null;
}

export function defaultChoiceOptions(acroformName: string): string[] | null {
  return CHOICE_OPTIONS_BY_ACROFORM[acroformName] ?? null;
}

export function normalizeTemplateField(field: PdfTemplateField): PdfTemplateField {
  const fieldType = resolveFieldType(field.acroformName, field.fieldType);
  const systemVariable = field.systemVariable ?? suggestSystemVariable(field.acroformName);
  const choiceOptions =
    fieldType === "choice"
      ? (field.choiceOptions ?? defaultChoiceOptions(field.acroformName))
      : null;

  return {
    ...field,
    fieldType,
    systemVariable,
    choiceOptions,
  };
}

export function normalizeTemplateFields(fields: PdfTemplateField[]): PdfTemplateField[] {
  return fields.map(normalizeTemplateField);
}
