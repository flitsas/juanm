import type { FieldMappingInput, PdfTemplateField, PlantillaFieldType } from "./plantillas.types";

export function isMappingComplete(fields: PdfTemplateField[]): boolean {
  return fields.every((field) => Boolean(field.systemVariable?.trim()));
}

export function buildMappingsFromFields(
  fields: PdfTemplateField[],
  overrides: Record<string, Partial<FieldMappingInput>>,
): FieldMappingInput[] {
  return fields.map((field) => {
    const override = overrides[field.id] ?? {};
    return {
      fieldId: field.id,
      fieldType: (override.fieldType ?? field.fieldType) as PlantillaFieldType,
      systemVariable: override.systemVariable ?? field.systemVariable ?? "",
      choiceOptions:
        override.choiceOptions ??
        (field.fieldType === "choice" ? (field.choiceOptions ?? []) : null),
    };
  });
}

export function validateMappings(mappings: FieldMappingInput[]): string | null {
  for (const mapping of mappings) {
    if (!mapping.systemVariable.trim()) {
      return "Todos los tags deben tener una variable del sistema asignada.";
    }
    if (
      mapping.fieldType === "choice" &&
      (!mapping.choiceOptions || mapping.choiceOptions.length === 0)
    ) {
      return `El tag requiere opciones de lista para tipo selección.`;
    }
  }
  return null;
}
