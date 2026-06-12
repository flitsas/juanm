"use client";

import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { useMemo, useState } from "react";
import { FlitSelect } from "@/components/flit/flit-select";
import { defaultChoiceOptions } from "../lib/acroform-defaults";
import { buildMappingsFromFields, validateMappings } from "../lib/mapping-validation";
import {
  FIELD_TYPE_LABELS,
  type FieldMappingInput,
  type PdfTemplateField,
  type PlantillaFieldType,
  type SystemVariable,
} from "../lib/plantillas.types";

type TemplateMappingPanelProps = {
  templateName: string;
  fields: PdfTemplateField[];
  systemVariables: SystemVariable[];
  saving: boolean;
  activating: boolean;
  onSave: (mappings: FieldMappingInput[]) => Promise<void>;
  onActivate: () => Promise<void>;
  onCancel: () => void;
};

type FieldOverride = Partial<FieldMappingInput>;

export function TemplateMappingPanel({
  templateName,
  fields,
  systemVariables,
  saving,
  activating,
  onSave,
  onActivate,
  onCancel,
}: TemplateMappingPanelProps) {
  const [overrides, setOverrides] = useState<Record<string, FieldOverride>>({});
  const [error, setError] = useState<string | null>(null);

  const fieldTypeOptions = useMemo(
    () =>
      (Object.keys(FIELD_TYPE_LABELS) as PlantillaFieldType[]).map((value) => ({
        label: FIELD_TYPE_LABELS[value],
        value,
      })),
    [],
  );

  const variableOptions = useMemo(
    () =>
      systemVariables.map((variable) => ({
        label: `${variable.label} (${variable.key})`,
        value: variable.key,
      })),
    [systemVariables],
  );

  const updateOverride = (fieldId: string, patch: FieldOverride) => {
    setOverrides((prev) => ({ ...prev, [fieldId]: { ...prev[fieldId], ...patch } }));
  };

  const handleSave = async () => {
    const mappings = buildMappingsFromFields(fields, overrides);
    const validationError = validateMappings(mappings, systemVariables);
    if (validationError) {
      setError(validationError);
      return;
    }
    setError(null);
    await onSave(mappings);
  };

  const handleActivate = async () => {
    const mappings = buildMappingsFromFields(fields, overrides);
    const validationError = validateMappings(mappings, systemVariables);
    if (validationError) {
      setError(validationError);
      return;
    }
    setError(null);
    await onSave(mappings);
    await onActivate();
  };

  return (
    <div className="space-y-4" data-testid="gdc-template-mapping-panel">
      <div className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6">
        <h2 className="text-lg font-semibold text-[var(--deep)]">
          Mapeo de campos — {templateName}
        </h2>
        <p className="mt-1 text-sm text-[var(--muted-foreground)]">
          {fields.length} tags extraídos. Asigne tipo polimórfico y variable del sistema por tag.
        </p>
      </div>

      <div
        className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
        data-testid="gdc-extracted-tags-table"
      >
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
              <th className="px-4 py-3">Tag AcroForm</th>
              <th className="px-4 py-3">Tipo</th>
              <th className="px-4 py-3">Variable sistema</th>
              <th className="px-4 py-3">Opciones lista</th>
            </tr>
          </thead>
          <tbody>
            {fields.map((field) => {
              const override = overrides[field.id] ?? {};
              const fieldType = (override.fieldType ?? field.fieldType) as PlantillaFieldType;
              const systemVariable = override.systemVariable ?? field.systemVariable ?? "";
              const choiceText =
                override.choiceOptions?.join(", ") ?? field.choiceOptions?.join(", ") ?? "";

              return (
                <tr key={field.id} className="border-t border-[var(--border)]/60">
                  <td className="px-4 py-3 font-mono text-xs text-[var(--deep)]">
                    {field.acroformName}
                  </td>
                  <td className="px-4 py-3">
                    <FlitSelect
                      value={fieldType}
                      options={fieldTypeOptions}
                      onChange={(value) =>
                        updateOverride(field.id, { fieldType: value as PlantillaFieldType })
                      }
                      className="max-w-[180px]"
                      data-testid={`gdc-field-type-${field.acroformName}`}
                    />
                  </td>
                  <td className="px-4 py-3">
                    <FlitSelect
                      value={systemVariable}
                      options={variableOptions}
                      onChange={(nextVariable) => {
                        const variableDef = systemVariables.find((v) => v.key === nextVariable);
                        const patch: FieldOverride = { systemVariable: nextVariable };
                        if (variableDef) {
                          patch.fieldType = variableDef.dataType;
                          if (
                            variableDef.dataType === "choice" &&
                            !(override.choiceOptions?.length || field.choiceOptions?.length)
                          ) {
                            patch.choiceOptions = defaultChoiceOptions(field.acroformName) ?? [];
                          }
                        }
                        updateOverride(field.id, patch);
                      }}
                      placeholder="Seleccionar variable"
                      className="max-w-[280px]"
                      data-testid={`gdc-field-variable-${field.acroformName}`}
                    />
                  </td>
                  <td className="px-4 py-3">
                    {fieldType === "choice" ? (
                      <InputText
                        value={choiceText}
                        onChange={(e) =>
                          updateOverride(field.id, {
                            choiceOptions: e.target.value
                              .split(",")
                              .map((v) => v.trim())
                              .filter(Boolean),
                          })
                        }
                        className="flit-field-input w-full max-w-[240px]"
                        placeholder="Opción 1, Opción 2"
                      />
                    ) : (
                      <span className="text-[var(--muted-foreground)]">—</span>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {error ? (
        <div
          className="rounded-xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-4 text-sm text-[var(--alert)]"
          role="alert"
        >
          {error}
        </div>
      ) : null}

      <div className="flex justify-end gap-2">
        <Button type="button" label="Cancelar" className="flit-btn-secondary" onClick={onCancel} />
        <Button
          type="button"
          label="Guardar mapeo"
          className="flit-btn-secondary"
          loading={saving}
          onClick={() => void handleSave()}
          data-testid="gdc-save-mapping-btn"
        />
        <Button
          type="button"
          label="Activar plantilla"
          className="flit-btn-primary"
          loading={activating || saving}
          onClick={() => void handleActivate()}
          data-testid="gdc-activate-template-btn"
        />
      </div>
    </div>
  );
}
