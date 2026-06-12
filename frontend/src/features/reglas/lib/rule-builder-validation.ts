import { countPredicates } from "./condition-tree";
import type { ConditionNode } from "./reglas.types";

export type RuleBuilderFormState = {
  name: string;
  description: string;
  isActive: boolean;
  pdfTemplateId: string;
  emailSubject: string;
  emailBodyHtml: string;
  secretariatContactId: string;
  conditionRoot: ConditionNode;
};

export const EMPTY_RULE_BUILDER_FORM: RuleBuilderFormState = {
  name: "",
  description: "",
  isActive: true,
  pdfTemplateId: process.env.NEXT_PUBLIC_DEFAULT_PDF_TEMPLATE_ID ?? "",
  emailSubject: "DP {{numero_comparendo}}",
  emailBodyHtml: "<p>Adjunto derecho de petición para {{infractor}}.</p>",
  secretariatContactId: "",
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
};

export type RuleBuilderFormErrors = Partial<
  Record<
    | "name"
    | "pdfTemplateId"
    | "emailSubject"
    | "emailBodyHtml"
    | "conditionRoot"
    | "secretariatContactId",
    string
  >
>;

export function hasRuleBuilderErrors(errors: RuleBuilderFormErrors): boolean {
  return Object.keys(errors).length > 0;
}

export function validateRuleBuilderForm(form: RuleBuilderFormState): RuleBuilderFormErrors {
  const errors: RuleBuilderFormErrors = {};

  if (!form.name.trim()) {
    errors.name = "El nombre es obligatorio.";
  }

  if (!form.pdfTemplateId.trim()) {
    errors.pdfTemplateId = "Indique el identificador de plantilla PDF (GDC).";
  }

  if (!form.emailSubject.trim()) {
    errors.emailSubject = "El asunto del correo es obligatorio.";
  }

  if (!form.emailBodyHtml.trim()) {
    errors.emailBodyHtml = "El cuerpo HTML del correo es obligatorio.";
  }

  if (form.isActive && countPredicates(form.conditionRoot) === 0) {
    errors.conditionRoot = "Una regla activa debe tener al menos una condición.";
  }

  if (form.isActive && hasEmptyPredicate(form.conditionRoot)) {
    errors.conditionRoot = "Complete campo, operador y valor en todas las condiciones.";
  }

  return errors;
}

function hasEmptyPredicate(node: ConditionNode): boolean {
  if (node.nodeType === "predicate") {
    return (
      !node.fieldKey?.trim() ||
      !node.comparisonOperator?.trim() ||
      node.comparisonValue === undefined ||
      node.comparisonValue === null ||
      String(node.comparisonValue).trim() === ""
    );
  }

  return (node.children ?? []).some(hasEmptyPredicate);
}
