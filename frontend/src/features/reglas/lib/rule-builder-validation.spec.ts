import { describe, expect, it } from "vitest";
import { countPredicates, formatConditionSummary } from "./condition-tree";
import {
  EMPTY_RULE_BUILDER_FORM,
  hasRuleBuilderErrors,
  normalizeSecretariatContactId,
  validateRuleBuilderForm,
} from "./rule-builder-validation";

describe("validateRuleBuilderForm", () => {
  it("exige nombre y plantilla PDF", () => {
    const errors = validateRuleBuilderForm({
      ...EMPTY_RULE_BUILDER_FORM,
      name: "",
      pdfTemplateId: "",
    });
    expect(hasRuleBuilderErrors(errors)).toBe(true);
    expect(errors.name).toBeTruthy();
    expect(errors.pdfTemplateId).toBeTruthy();
  });

  it("exige condiciones completas en regla activa", () => {
    const errors = validateRuleBuilderForm({
      ...EMPTY_RULE_BUILDER_FORM,
      name: "DP Bogotá",
      pdfTemplateId: "33333333-3333-3333-3333-333333333333",
      isActive: true,
      conditionRoot: {
        nodeType: "group",
        logicOperator: "and",
        children: [
          {
            nodeType: "predicate",
            fieldKey: "estado",
            comparisonOperator: "eq",
            comparisonValue: "",
          },
        ],
      },
    });
    expect(errors.conditionRoot).toBeTruthy();
  });

  it("acepta regla activa con predicado completo", () => {
    const errors = validateRuleBuilderForm({
      ...EMPTY_RULE_BUILDER_FORM,
      name: "DP Bogotá",
      pdfTemplateId: "33333333-3333-3333-3333-333333333333",
      isActive: true,
      conditionRoot: {
        nodeType: "group",
        logicOperator: "and",
        children: [
          {
            nodeType: "predicate",
            fieldKey: "estado",
            comparisonOperator: "eq",
            comparisonValue: "Notificado",
          },
        ],
      },
    });
    expect(hasRuleBuilderErrors(errors)).toBe(false);
  });
});

describe("normalizeSecretariatContactId", () => {
  it("convierte opción vacía del dropdown a null", () => {
    expect(
      normalizeSecretariatContactId({
        label: "Sin contacto fijo (resolver por secretaría)",
        value: "",
      }),
    ).toBeNull();
  });

  it("extrae el id cuando el dropdown devuelve objeto", () => {
    expect(
      normalizeSecretariatContactId({
        label: "Bogotá — ops@test.co",
        value: "019ebcdb-4846-7cbf-8461-7241b3283907",
      }),
    ).toBe("019ebcdb-4846-7cbf-8461-7241b3283907");
  });
});

describe("condition-tree helpers", () => {
  it("cuenta predicados en árbol anidado", () => {
    const root = {
      nodeType: "group",
      logicOperator: "or",
      children: [
        {
          nodeType: "group",
          logicOperator: "and",
          children: [
            {
              nodeType: "predicate",
              fieldKey: "estado",
              comparisonOperator: "eq",
              comparisonValue: "A",
            },
            {
              nodeType: "predicate",
              fieldKey: "placa",
              comparisonOperator: "contains",
              comparisonValue: "ABC",
            },
          ],
        },
      ],
    };
    expect(countPredicates(root)).toBe(2);
    expect(formatConditionSummary(root)).toContain("2 condición");
  });
});
