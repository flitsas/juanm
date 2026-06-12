"use client";

import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { FlitSelect } from "@/components/flit/flit-select";
import { createGroupNode, createPredicateNode } from "../lib/condition-tree";
import {
  REGLAS_FIELD_OPTIONS,
  REGLAS_LOGIC_OPTIONS,
  REGLAS_NODE_TYPES,
  REGLAS_OPERATOR_OPTIONS,
} from "../lib/reglas.constants";
import type { ConditionNode } from "../lib/reglas.types";

type ConditionTreeEditorProps = {
  root: ConditionNode;
  onChange: (root: ConditionNode) => void;
  depth?: number;
};

export function ConditionTreeEditor({ root, onChange, depth = 0 }: ConditionTreeEditorProps) {
  if (root.nodeType === REGLAS_NODE_TYPES.predicate) {
    return (
      <PredicateRow
        node={root}
        onChange={onChange}
        onRemove={depth === 0 ? undefined : () => onChange(createPredicateNode())}
      />
    );
  }

  const children = root.children ?? [];

  const updateChild = (index: number, child: ConditionNode) => {
    const next = [...children];
    next[index] = child;
    onChange({ ...root, children: next });
  };

  const removeChild = (index: number) => {
    const next = children.filter((_, i) => i !== index);
    onChange({ ...root, children: next.length > 0 ? next : [createPredicateNode()] });
  };

  return (
    <div
      className="space-y-3 rounded-xl border border-[var(--border)] bg-[var(--muted)]/20 p-4"
      data-testid={depth === 0 ? "reglas-condition-root" : "reglas-condition-group"}
    >
      <div className="flex flex-wrap items-center gap-3">
        <span className="text-xs font-semibold uppercase tracking-wide text-[var(--muted-foreground)]">
          Agrupación
        </span>
        <FlitSelect
          value={root.logicOperator ?? "and"}
          options={REGLAS_LOGIC_OPTIONS}
          onChange={(value) => onChange({ ...root, logicOperator: value })}
          className="w-40"
          aria-label="Operador lógico"
        />
        <Button
          type="button"
          label="Condición"
          className="flit-btn-secondary p-button-sm"
          onClick={() => onChange({ ...root, children: [...children, createPredicateNode()] })}
        />
        <Button
          type="button"
          label="Subgrupo"
          className="flit-btn-secondary p-button-sm"
          onClick={() => onChange({ ...root, children: [...children, createGroupNode()] })}
        />
      </div>

      <div className="space-y-3 pl-2">
        {children.map((child, index) => (
          <div
            // biome-ignore lint/suspicious/noArrayIndexKey: nodos del árbol no tienen id estable hasta persistir
            key={`${depth}-${index}-${child.nodeType}-${child.fieldKey ?? child.logicOperator ?? ""}`}
            className="relative"
          >
            {child.nodeType === REGLAS_NODE_TYPES.group ? (
              <div className="space-y-2">
                <ConditionTreeEditor
                  root={child}
                  depth={depth + 1}
                  onChange={(updated) => updateChild(index, updated)}
                />
                {children.length > 1 ? (
                  <Button
                    type="button"
                    label="Quitar grupo"
                    className="flit-btn-danger p-button-sm"
                    onClick={() => removeChild(index)}
                  />
                ) : null}
              </div>
            ) : (
              <PredicateRow
                node={child}
                onChange={(updated) => updateChild(index, updated)}
                onRemove={children.length > 1 ? () => removeChild(index) : undefined}
              />
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

function PredicateRow({
  node,
  onChange,
  onRemove,
}: {
  node: ConditionNode;
  onChange: (node: ConditionNode) => void;
  onRemove?: () => void;
}) {
  return (
    <div
      className="grid gap-3 rounded-lg border border-[var(--border)]/70 bg-[var(--card)] p-3 md:grid-cols-[1fr_1fr_1fr_auto]"
      data-testid="reglas-condition-predicate"
    >
      <FlitSelect
        value={node.fieldKey ?? "estado"}
        options={REGLAS_FIELD_OPTIONS}
        onChange={(value) => onChange({ ...node, fieldKey: value })}
        aria-label="Campo"
      />
      <FlitSelect
        value={node.comparisonOperator ?? "eq"}
        options={REGLAS_OPERATOR_OPTIONS}
        onChange={(value) => onChange({ ...node, comparisonOperator: value })}
        aria-label="Operador"
      />
      <InputText
        value={node.comparisonValue ?? ""}
        onChange={(e) => onChange({ ...node, comparisonValue: e.target.value })}
        className="flit-field-input w-full"
        placeholder="Valor"
        aria-label="Valor de comparación"
      />
      {onRemove ? (
        <Button
          type="button"
          icon="pi pi-times"
          className="flit-btn-danger p-button-sm p-button-rounded"
          onClick={onRemove}
          aria-label="Quitar condición"
        />
      ) : (
        <span className="hidden md:block" aria-hidden />
      )}
    </div>
  );
}
