"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { InputText } from "primereact/inputtext";
import { InputTextarea } from "primereact/inputtextarea";
import { useEffect, useMemo, useState } from "react";
import { FlitSelect } from "@/components/flit/flit-select";
import { FlitFormField, FlitModal, FlitModalActions } from "@/components/flit/modal-form";
import { usePdfTemplates } from "@/features/plantillas/api/use-plantillas";
import { PdfTemplateDropdown } from "@/features/plantillas/components/PdfTemplateDropdown";
import { pickDefaultPdfTemplateId } from "@/features/plantillas/lib/template-select-options";
import { createDefaultConditionRoot } from "../lib/condition-tree";
import type {
  ReglasRule,
  ReglasSecretariatContact,
  SaveReglasRulePayload,
} from "../lib/reglas.types";
import {
  EMPTY_RULE_BUILDER_FORM,
  hasRuleBuilderErrors,
  normalizeSecretariatContactId,
  type RuleBuilderFormState,
  validateRuleBuilderForm,
} from "../lib/rule-builder-validation";
import { ConditionTreeEditor } from "./ConditionTreeEditor";

type ReglasRuleFormDialogProps = {
  open: boolean;
  rule: ReglasRule | null;
  contacts: ReglasSecretariatContact[];
  saving?: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (payload: SaveReglasRulePayload) => void;
};

export function ReglasRuleFormDialog({
  open,
  rule,
  contacts,
  saving,
  onOpenChange,
  onSubmit,
}: ReglasRuleFormDialogProps) {
  const [form, setForm] = useState<RuleBuilderFormState>(EMPTY_RULE_BUILDER_FORM);
  const [errors, setErrors] = useState<ReturnType<typeof validateRuleBuilderForm>>({});
  const templatesQuery = usePdfTemplates();

  const contactOptions = useMemo(
    () => [
      { label: "Sin contacto fijo (resolver por secretaría)", value: "" },
      ...contacts
        .filter((c) => c.isActive)
        .map((c) => ({
          label: `${c.secretariatName} — ${c.contactEmail}`,
          value: c.id,
        })),
    ],
    [contacts],
  );

  useEffect(() => {
    if (!open) return;
    setForm({
      name: rule?.name ?? "",
      description: rule?.description ?? "",
      isActive: rule?.isActive ?? true,
      pdfTemplateId: rule?.pdfTemplateId ?? EMPTY_RULE_BUILDER_FORM.pdfTemplateId,
      emailSubject: rule?.emailSubject ?? EMPTY_RULE_BUILDER_FORM.emailSubject,
      emailBodyHtml: rule?.emailBodyHtml ?? EMPTY_RULE_BUILDER_FORM.emailBodyHtml,
      secretariatContactId: rule?.secretariatContactId ?? "",
      conditionRoot: rule?.conditionRoot ?? createDefaultConditionRoot(),
    });
    setErrors({});
  }, [open, rule]);

  useEffect(() => {
    if (!open || rule || templatesQuery.isLoading || !templatesQuery.data) return;
    const defaultId = pickDefaultPdfTemplateId(templatesQuery.data.items);
    if (!defaultId) return;
    setForm((prev) => (prev.pdfTemplateId.trim() ? prev : { ...prev, pdfTemplateId: defaultId }));
  }, [open, rule, templatesQuery.isLoading, templatesQuery.data]);

  const update = (patch: Partial<RuleBuilderFormState>) => {
    setForm((prev) => ({ ...prev, ...patch }));
    setErrors({});
  };

  const handleSubmit = () => {
    const validation = validateRuleBuilderForm(form);
    setErrors(validation);
    if (hasRuleBuilderErrors(validation)) return;

    onSubmit({
      name: form.name.trim(),
      description: form.description.trim() || null,
      isActive: form.isActive,
      pdfTemplateId: form.pdfTemplateId.trim(),
      emailSubject: form.emailSubject.trim(),
      emailBodyHtml: form.emailBodyHtml,
      secretariatContactId: normalizeSecretariatContactId(form.secretariatContactId),
      conditionRoot: form.conditionRoot,
    });
  };

  return (
    <FlitModal
      open={open}
      onClose={() => onOpenChange(false)}
      title={rule ? "Editar regla dinámica" : "Nueva regla de Derecho de Petición"}
      titleId="reglas-rule-form-title"
      testId="reglas-rule-form-dialog"
      size="xl"
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
            label={rule ? "Guardar" : "Crear regla"}
            className="flit-btn-primary"
            loading={saving}
            onClick={handleSubmit}
            data-testid="reglas-rule-save-btn"
          />
        </FlitModalActions>
      }
    >
      <div className="space-y-4">
        <FlitFormField label="Nombre" htmlFor="reglas-rule-name" error={errors.name}>
          <InputText
            id="reglas-rule-name"
            value={form.name}
            onChange={(e) => update({ name: e.target.value })}
            className="flit-field-input w-full"
          />
        </FlitFormField>

        <FlitFormField label="Descripción" htmlFor="reglas-rule-description">
          <InputText
            id="reglas-rule-description"
            value={form.description}
            onChange={(e) => update({ description: e.target.value })}
            className="flit-field-input w-full"
          />
        </FlitFormField>

        <div className="flex items-center gap-2">
          <Checkbox
            inputId="reglas-rule-active"
            checked={form.isActive}
            onChange={(e) => update({ isActive: Boolean(e.checked) })}
          />
          <label htmlFor="reglas-rule-active" className="text-sm text-[var(--deep)]">
            Regla activa
          </label>
        </div>

        <FlitFormField label="Plantilla PDF (GDC)" error={errors.pdfTemplateId}>
          <PdfTemplateDropdown
            value={form.pdfTemplateId}
            onChange={(pdfTemplateId) => update({ pdfTemplateId })}
            inputId="reglas-pdf-template"
            testId="reglas-pdf-template-dropdown"
          />
        </FlitFormField>

        <FlitFormField
          label="Contacto secretaría (opcional)"
          htmlFor="reglas-secretariat-contact"
          error={errors.secretariatContactId}
        >
          <FlitSelect
            inputId="reglas-secretariat-contact"
            value={form.secretariatContactId}
            options={contactOptions}
            onChange={(value) =>
              update({ secretariatContactId: normalizeSecretariatContactId(value) ?? "" })
            }
            placeholder="Seleccione contacto"
          />
        </FlitFormField>

        <FlitFormField
          label="Asunto correo"
          htmlFor="reglas-email-subject"
          error={errors.emailSubject}
        >
          <InputText
            id="reglas-email-subject"
            value={form.emailSubject}
            onChange={(e) => update({ emailSubject: e.target.value })}
            className="flit-field-input w-full"
          />
        </FlitFormField>

        <FlitFormField label="Cuerpo HTML" htmlFor="reglas-email-body" error={errors.emailBodyHtml}>
          <InputTextarea
            id="reglas-email-body"
            value={form.emailBodyHtml}
            onChange={(e) => update({ emailBodyHtml: e.target.value })}
            rows={4}
            className="flit-textarea w-full font-mono text-xs"
          />
        </FlitFormField>

        <div>
          <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-[var(--muted-foreground)]">
            Constructor de condiciones
          </p>
          {errors.conditionRoot ? (
            <p className="mb-2 text-sm text-[var(--alert)]" role="alert">
              {errors.conditionRoot}
            </p>
          ) : null}
          <ConditionTreeEditor
            root={form.conditionRoot}
            onChange={(conditionRoot) => update({ conditionRoot })}
          />
        </div>
      </div>
    </FlitModal>
  );
}
