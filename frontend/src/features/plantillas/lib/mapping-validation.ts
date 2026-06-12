import type {
  FieldMappingInput,
  PdfTemplateField,
  PlantillaFieldType,
  SystemVariable,
} from "./plantillas.types";

export function isFieldTypeCompatible(
  fieldType: PlantillaFieldType,
  variableDataType: PlantillaFieldType,
): boolean {
  return fieldType === variableDataType;
}

export function isMappingComplete(fields: PdfTemplateField[]): boolean {
  return fields.every((field) => Boolean(field.systemVariable?.trim()));
}

export function buildMappingsFromFields(
  fields: PdfTemplateField[],
  overrides: Record<string, Partial<FieldMappingInput>>,
): FieldMappingInput[] {
  return fields.map((field) => {
    const override = overrides[field.id] ?? {};
    const fieldType = (override.fieldType ?? field.fieldType) as PlantillaFieldType;
    return {
      fieldId: field.id,
      fieldType,
      systemVariable: override.systemVariable ?? field.systemVariable ?? "",
      choiceOptions:
        override.choiceOptions ?? (fieldType === "choice" ? (field.choiceOptions ?? []) : null),
    };
  });
}

export function validateMappings(
  mappings: FieldMappingInput[],
  systemVariables: SystemVariable[] = [],
): string | null {
  const variableTypes = new Map(
    systemVariables.map((variable) => [variable.key, variable.dataType]),
  );

  for (const mapping of mappings) {
    if (!mapping.systemVariable.trim()) {
      return "Todos los tags deben tener una variable del sistema asignada.";
    }

    const variableDataType = variableTypes.get(mapping.systemVariable);
    if (variableDataType && !isFieldTypeCompatible(mapping.fieldType, variableDataType)) {
      return `El tipo '${mapping.fieldType}' no es compatible con la variable '${mapping.systemVariable}' (${variableDataType}).`;
    }

    if (
      mapping.fieldType === "choice" &&
      (!mapping.choiceOptions || mapping.choiceOptions.length === 0)
    ) {
      return "El tag requiere opciones de lista para tipo selección.";
    }
  }
  return null;
}
