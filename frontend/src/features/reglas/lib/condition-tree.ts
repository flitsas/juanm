import { REGLAS_LOGIC_OPERATORS, REGLAS_NODE_TYPES } from "./reglas.constants";
import type { ConditionNode } from "./reglas.types";

export function createDefaultConditionRoot(): ConditionNode {
  return {
    nodeType: REGLAS_NODE_TYPES.group,
    logicOperator: REGLAS_LOGIC_OPERATORS.and,
    children: [
      {
        nodeType: REGLAS_NODE_TYPES.predicate,
        fieldKey: "estado",
        comparisonOperator: "eq",
        comparisonValue: "",
      },
    ],
  };
}

export function createPredicateNode(): ConditionNode {
  return {
    nodeType: REGLAS_NODE_TYPES.predicate,
    fieldKey: "estado",
    comparisonOperator: "eq",
    comparisonValue: "",
  };
}

export function createGroupNode(): ConditionNode {
  return {
    nodeType: REGLAS_NODE_TYPES.group,
    logicOperator: REGLAS_LOGIC_OPERATORS.and,
    children: [createPredicateNode()],
  };
}

export function countPredicates(root: ConditionNode | null | undefined): number {
  if (!root) return 0;
  if (root.nodeType === REGLAS_NODE_TYPES.predicate) return 1;
  return (root.children ?? []).reduce((sum, child) => sum + countPredicates(child), 0);
}

export function formatConditionSummary(root: ConditionNode | null | undefined): string {
  const count = countPredicates(root);
  if (count === 0) return "Sin condiciones";
  const logic =
    root?.nodeType === REGLAS_NODE_TYPES.group
      ? root.logicOperator === REGLAS_LOGIC_OPERATORS.or
        ? "O"
        : "Y"
      : "";
  return logic ? `${count} condición(es) · ${logic}` : `${count} condición(es)`;
}
