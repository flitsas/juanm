"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { InputNumber } from "primereact/inputnumber";
import { InputText } from "primereact/inputtext";
import { useEffect, useMemo, useState } from "react";
import { FlitSelect } from "@/components/flit/flit-select";
import { FlitFormField, FlitModal, FlitModalActions } from "@/components/flit/modal-form";
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
    <FlitModal
      open={open}
      onClose={() => onOpenChange(false)}
      title={rule ? "Editar regla" : "Nueva regla de comunicación"}
      titleId="rule-form-title"
      testId="notif-rule-form-dialog"
      footer={
        <FlitModalActions>
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
        </FlitModalActions>
      }
    >
      <div className="space-y-4">
        <FlitFormField label="Nombre" htmlFor="rule-name" error={errors.name}>
          <InputText
            id="rule-name"
            value={form.name}
            onChange={(e) => update({ name: e.target.value })}
            className="flit-field-input w-full"
            aria-invalid={Boolean(errors.name)}
          />
        </FlitFormField>

        <FlitFormField label="Plantilla" htmlFor="rule-template" error={errors.emailTemplateId}>
          <FlitSelect
            inputId="rule-template"
            value={form.emailTemplateId}
            options={templateOptions}
            onChange={(value) => update({ emailTemplateId: value })}
            placeholder="Seleccione plantilla"
            invalid={Boolean(errors.emailTemplateId)}
          />
        </FlitFormField>

        <FlitFormField
          label="Tipo de disparador"
          htmlFor="rule-trigger-type"
          error={errors.triggerType}
        >
          <FlitSelect
            inputId="rule-trigger-type"
            value={form.triggerType}
            options={TRIGGER_OPTIONS}
            onChange={(value) => update({ triggerType: value as RuleFormState["triggerType"] })}
            placeholder="Seleccione tipo"
            invalid={Boolean(errors.triggerType)}
          />
        </FlitFormField>

        {form.triggerType === "chronological" ? (
          <div className="grid gap-4 sm:grid-cols-2">
            <FlitFormField label="Días" htmlFor="rule-days" error={errors.triggerDays}>
              <InputNumber
                inputId="rule-days"
                value={Number.parseInt(form.triggerDays, 10) || 0}
                onValueChange={(e) => update({ triggerDays: String(e.value ?? 0) })}
                min={0}
                className="flit-input-number w-full"
                inputClassName="flit-field-input w-full"
              />
            </FlitFormField>
            <FlitFormField label="Referencia" htmlFor="rule-reference">
              <FlitSelect
                inputId="rule-reference"
                value={form.triggerReference ?? "fecha_notificacion"}
                options={REFERENCE_OPTIONS}
                onChange={(value) =>
                  update({ triggerReference: value as RuleFormState["triggerReference"] })
                }
              />
            </FlitFormField>
          </div>
        ) : null}

        {form.triggerType === "state" ? (
          <FlitFormField
            label="Estado del comparendo"
            htmlFor="rule-estado"
            error={errors.triggerEstado}
          >
            <InputText
              id="rule-estado"
              value={form.triggerEstado}
              onChange={(e) => update({ triggerEstado: e.target.value })}
              placeholder="Notificado"
              className="flit-field-input w-full"
              aria-invalid={Boolean(errors.triggerEstado)}
            />
          </FlitFormField>
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
    </FlitModal>
  );
}
