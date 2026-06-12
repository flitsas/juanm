import { describe, expect, it } from "vitest";
import { buildMappingsFromFields, isMappingComplete, validateMappings } from "./mapping-validation";
import type { PdfTemplateField } from "./plantillas.types";

// Uso de ejemplo: validateMappings(buildMappingsFromFields(fields, overrides))

const sampleFields: PdfTemplateField[] = [
  {
    id: "f1",
    acroformName: "comparendo_numero",
    fieldType: "text",
    systemVariable: "comparendo.numero",
    choiceOptions: null,
    sortOrder: 0,
  },
  {
    id: "f2",
    acroformName: "comparendo_estado",
    fieldType: "choice",
    systemVariable: null,
    choiceOptions: null,
    sortOrder: 1,
  },
];

describe("mapping-validation", () => {
  it("isMappingComplete detecta tags sin variable", () => {
    expect(isMappingComplete(sampleFields)).toBe(false);
    expect(
      isMappingComplete([
        { ...sampleFields[0] },
        { ...sampleFields[1], systemVariable: "comparendo.estado" },
      ]),
    ).toBe(true);
  });

  it("validateMappings exige opciones para tipo choice", () => {
    const mappings = buildMappingsFromFields(sampleFields, {
      f2: {
        fieldId: "f2",
        fieldType: "choice",
        systemVariable: "comparendo.estado",
        choiceOptions: [],
      },
    });
    expect(validateMappings(mappings)).toContain("opciones");
  });

  it("buildMappingsFromFields conserva overrides de variable", () => {
    const mappings = buildMappingsFromFields(sampleFields, {
      f2: { systemVariable: "comparendo.estado", choiceOptions: ["A", "B"] },
    });
    expect(mappings[1].systemVariable).toBe("comparendo.estado");
    expect(mappings[1].choiceOptions).toEqual(["A", "B"]);
  });
});
