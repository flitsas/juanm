"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { Dropdown } from "primereact/dropdown";
import { InputNumber } from "primereact/inputnumber";
import { InputText } from "primereact/inputtext";
import { useEffect, useMemo, useState } from "react";
import type { NotifRule, NotifTemplate, TriggerReference } from "../lib/notif.types";
import {
  EMPTY_RULE_FORM,
  hasRuleErrors,
  type RuleFormState,
  validateRuleForm,
} from "../lib/rule-validation";

type RuleFormDialogProps = {
  open: boolean;
  rule: NotifRule | null;
  templates: NotifTemplate[];
  saving?: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (payload: {
    emailTemplateId: string;
    name: string;
    triggerType: "chronological" | "state";
    triggerDays?: number | null;
    triggerReference?: TriggerReference | null;
    triggerEstado?: string | null;
    isActive: boolean;
  }) => void;
};

const TRIGGER_OPTIONS = [
  { label: "Cronológico (días)", value: "chronological" },
  { label: "Por estado del comparendo", value: "state" },
];

const REFERENCE_OPTIONS = [
  { label: "Fecha comparendo", value: "fecha_comparendo" },
  { label: "Fecha notificación", value: "fecha_notificacion" },
];

export function RuleFormDialog({
  open,
  rule,
  templates,
  saving,
  onOpenChange,
  onSubmit,
}: RuleFormDialogProps) {
  const [form, setForm] = useState<RuleFormState>(EMPTY_RULE_FORM);
  const [errors, setErrors] = useState<ReturnType<typeof validateRuleForm>>({});

  const templateOptions = useMemo(
    () => templates.map((t) => ({ label: t.name, value: t.id })),
    [templates],
  );

  useEffect(() => {
    if (!open) return;
    setForm({
      emailTemplateId: rule?.emailTemplateId ?? templates[0]?.id ?? "",
      name: rule?.name ?? "",
      triggerType: rule?.triggerType ?? "",
      triggerDays: String(rule?.triggerDays ?? 3),
      triggerReference: rule?.triggerReference ?? "fecha_notificacion",
      triggerEstado: rule?.triggerEstado ?? "",
      isActive: rule?.isActive ?? true,
    });
    setErrors({});
  }, [open, rule, templates]);

  if (!open) return null;

  const update = (patch: Partial<RuleFormState>) => {
    setForm((prev) => ({ ...prev, ...patch }));
    setErrors({});
  };

  const handleSubmit = () => {
    const validation = validateRuleForm(form);
    setErrors(validation);
    if (hasRuleErrors(validation) || !form.triggerType) return;

    onSubmit({
      emailTemplateId: form.emailTemplateId,
      name: form.name.trim(),
      triggerType: form.triggerType,
      triggerDays:
        form.triggerType === "chronological" ? Number.parseInt(form.triggerDays, 10) : null,
      triggerReference: form.triggerType === "chronological" ? form.triggerReference || null : null,
      triggerEstado: form.triggerType === "state" ? form.triggerEstado.trim() : null,
      isActive: form.isActive,
    });
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="rule-form-title"
      data-testid="notif-rule-form-dialog"
    >
      <div className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-[var(--card)] p-6 shadow-xl">
        <h2 id="rule-form-title" className="text-xl font-bold text-[var(--deep)]">
          {rule ? "Editar regla" : "Nueva regla de comunicación"}
        </h2>

        <div className="mt-6 space-y-4">
          <div className="flex flex-col gap-1">
            <label htmlFor="rule-name" className="text-xs text-[var(--muted-foreground)]">
              Nombre
            </label>
            <InputText
              id="rule-name"
              value={form.name}
              onChange={(e) => update({ name: e.target.value })}
              className="flit-field-input w-full"
              aria-invalid={Boolean(errors.name)}
            />
            {errors.name ? (
              <span className="text-xs text-[var(--alert)]" role="alert">
                {errors.name}
              </span>
            ) : null}
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="rule-template" className="text-xs text-[var(--muted-foreground)]">
              Plantilla
            </label>
            <Dropdown
              inputId="rule-template"
              value={form.emailTemplateId}
              options={templateOptions}
              onChange={(e) => update({ emailTemplateId: e.value as string })}
              placeholder="Seleccione plantilla"
              className="flit-dropdown w-full"
              panelClassName="flit-dropdown-panel"
              aria-invalid={Boolean(errors.emailTemplateId)}
            />
            {errors.emailTemplateId ? (
              <span className="text-xs text-[var(--alert)]" role="alert">
                {errors.emailTemplateId}
              </span>
            ) : null}
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="rule-trigger-type" className="text-xs text-[var(--muted-foreground)]">
              Tipo de disparador
            </label>
            <Dropdown
              inputId="rule-trigger-type"
              value={form.triggerType}
              options={TRIGGER_OPTIONS}
              onChange={(e) => update({ triggerType: e.value as RuleFormState["triggerType"] })}
              placeholder="Seleccione tipo"
              className="flit-dropdown w-full"
              panelClassName="flit-dropdown-panel"
            />
            {errors.triggerType ? (
              <span className="text-xs text-[var(--alert)]" role="alert">
                {errors.triggerType}
              </span>
            ) : null}
          </div>

          {form.triggerType === "chronological" ? (
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="flex flex-col gap-1">
                <label htmlFor="rule-days" className="text-xs text-[var(--muted-foreground)]">
                  Días
                </label>
                <InputNumber
                  inputId="rule-days"
                  value={Number.parseInt(form.triggerDays, 10) || 0}
                  onValueChange={(e) => update({ triggerDays: String(e.value ?? 0) })}
                  min={0}
                  className="w-full"
                />
                {errors.triggerDays ? (
                  <span className="text-xs text-[var(--alert)]" role="alert">
                    {errors.triggerDays}
                  </span>
                ) : null}
              </div>
              <div className="flex flex-col gap-1">
                <label htmlFor="rule-reference" className="text-xs text-[var(--muted-foreground)]">
                  Referencia
                </label>
                <Dropdown
                  inputId="rule-reference"
                  value={form.triggerReference}
                  options={REFERENCE_OPTIONS}
                  onChange={(e) =>
                    update({ triggerReference: e.value as RuleFormState["triggerReference"] })
                  }
                  className="flit-dropdown w-full"
                  panelClassName="flit-dropdown-panel"
                />
              </div>
            </div>
          ) : null}

          {form.triggerType === "state" ? (
            <div className="flex flex-col gap-1">
              <label htmlFor="rule-estado" className="text-xs text-[var(--muted-foreground)]">
                Estado del comparendo
              </label>
              <InputText
                id="rule-estado"
                value={form.triggerEstado}
                onChange={(e) => update({ triggerEstado: e.target.value })}
                placeholder="Notificado"
                className="flit-field-input w-full"
                aria-invalid={Boolean(errors.triggerEstado)}
              />
              {errors.triggerEstado ? (
                <span className="text-xs text-[var(--alert)]" role="alert">
                  {errors.triggerEstado}
                </span>
              ) : null}
            </div>
          ) : null}

          <div className="flex items-center gap-2">
            <Checkbox
              inputId="rule-active"
              checked={form.isActive}
              onChange={(e) => update({ isActive: Boolean(e.checked) })}
            />
            <label htmlFor="rule-active" className="text-sm text-[var(--deep)]">
              Regla activa
            </label>
          </div>
        </div>

        <div className="mt-6 flex justify-end gap-3">
          <Button
            type="button"
            label="Cancelar"
            className="flit-btn-secondary"
            onClick={() => onOpenChange(false)}
          />
          <Button
            type="button"
            label={rule ? "Guardar cambios" : "Crear regla"}
            className="flit-btn-primary"
            loading={saving}
            onClick={handleSubmit}
            data-testid="rule-form-submit"
          />
        </div>
      </div>
    </div>
  );
}
