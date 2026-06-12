"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { InputText } from "primereact/inputtext";
import { type ReactNode, useEffect, useState } from "react";
import type { ReglasSecretariatContact } from "../lib/reglas.types";
import {
  EMPTY_CONTACT_FORM,
  hasContactErrors,
  type ContactFormState,
  toContactPayload,
  validateContactForm,
} from "../lib/contact-validation";

type ContactFormDialogProps = {
  open: boolean;
  contact: ReglasSecretariatContact | null;
  saving?: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (payload: ReturnType<typeof toContactPayload>) => void;
};

export function ContactFormDialog({
  open,
  contact,
  saving,
  onOpenChange,
  onSubmit,
}: ContactFormDialogProps) {
  const [form, setForm] = useState<ContactFormState>(EMPTY_CONTACT_FORM);
  const [errors, setErrors] = useState<ReturnType<typeof validateContactForm>>({});

  useEffect(() => {
    if (!open) return;
    setForm({
      secretariatCode: contact?.secretariatCode ?? "",
      secretariatName: contact?.secretariatName ?? "",
      contactName: contact?.contactName ?? "",
      contactEmail: contact?.contactEmail ?? "",
      contactPhone: contact?.contactPhone ?? "",
      isActive: contact?.isActive ?? true,
    });
    setErrors({});
  }, [open, contact]);

  if (!open) return null;

  const update = (patch: Partial<ContactFormState>) => {
    setForm((prev) => ({ ...prev, ...patch }));
    setErrors({});
  };

  const handleSubmit = () => {
    const validation = validateContactForm(form);
    setErrors(validation);
    if (hasContactErrors(validation)) return;
    onSubmit(toContactPayload(form));
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="reglas-contact-form-title"
      data-testid="reglas-contact-form-dialog"
    >
      <div className="w-full max-w-lg rounded-2xl bg-[var(--card)] p-6 shadow-xl">
        <h2 id="reglas-contact-form-title" className="text-xl font-bold text-[var(--deep)]">
          {contact ? "Editar contacto secretaría" : "Nuevo contacto secretaría"}
        </h2>

        <div className="mt-6 space-y-4">
          <Field label="Código secretaría" error={errors.secretariatCode}>
            <InputText
              value={form.secretariatCode}
              onChange={(e) => update({ secretariatCode: e.target.value })}
              className="flit-field-input w-full"
              data-testid="reglas-contact-code"
            />
          </Field>
          <Field label="Nombre secretaría" error={errors.secretariatName}>
            <InputText
              value={form.secretariatName}
              onChange={(e) => update({ secretariatName: e.target.value })}
              className="flit-field-input w-full"
              data-testid="reglas-contact-secretariat-name"
            />
          </Field>
          <Field label="Nombre contacto" error={errors.contactName}>
            <InputText
              value={form.contactName}
              onChange={(e) => update({ contactName: e.target.value })}
              className="flit-field-input w-full"
              data-testid="reglas-contact-name"
            />
          </Field>
          <Field label="Correo" error={errors.contactEmail}>
            <InputText
              value={form.contactEmail}
              onChange={(e) => update({ contactEmail: e.target.value })}
              className="flit-field-input w-full"
              data-testid="reglas-contact-email"
            />
          </Field>
          <Field label="Teléfono" error={undefined}>
            <InputText
              value={form.contactPhone}
              onChange={(e) => update({ contactPhone: e.target.value })}
              className="flit-field-input w-full"
            />
          </Field>
          <div className="flex items-center gap-2">
            <Checkbox
              inputId="reglas-contact-active"
              checked={form.isActive}
              onChange={(e) => update({ isActive: Boolean(e.checked) })}
            />
            <label htmlFor="reglas-contact-active" className="text-sm text-[var(--deep)]">
              Contacto activo
            </label>
          </div>
        </div>

        <div className="mt-6 flex justify-end gap-2">
          <Button
            type="button"
            label="Cancelar"
            className="flit-btn-secondary"
            onClick={() => onOpenChange(false)}
          />
          <Button
            type="button"
            label={contact ? "Guardar" : "Crear"}
            className="flit-btn-primary"
            loading={saving}
            onClick={handleSubmit}
            data-testid="reglas-contact-save-btn"
          />
        </div>
      </div>
    </div>
  );
}

function Field({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: ReactNode;
}) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs text-[var(--muted-foreground)]">{label}</span>
      {children}
      {error ? (
        <span className="text-xs text-[var(--alert)]" role="alert">
          {error}
        </span>
      ) : null}
    </div>
  );
}
