"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { InputText } from "primereact/inputtext";
import { useEffect, useState } from "react";
import { FlitFormField, FlitModal, FlitModalActions } from "@/components/flit/modal-form";
import {
  type ContactFormState,
  EMPTY_CONTACT_FORM,
  hasContactErrors,
  toContactPayload,
  validateContactForm,
} from "../lib/contact-validation";
import type { ReglasSecretariatContact } from "../lib/reglas.types";

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
    <FlitModal
      open={open}
      onClose={() => onOpenChange(false)}
      title={contact ? "Editar contacto secretaría" : "Nuevo contacto secretaría"}
      titleId="reglas-contact-form-title"
      testId="reglas-contact-form-dialog"
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
            label={contact ? "Guardar" : "Crear"}
            className="flit-btn-primary"
            loading={saving}
            onClick={handleSubmit}
            data-testid="reglas-contact-save-btn"
          />
        </FlitModalActions>
      }
    >
      <div className="space-y-4">
        <FlitFormField label="Código secretaría" error={errors.secretariatCode}>
          <InputText
            value={form.secretariatCode}
            onChange={(e) => update({ secretariatCode: e.target.value })}
            className="flit-field-input w-full"
            data-testid="reglas-contact-code"
          />
        </FlitFormField>
        <FlitFormField label="Nombre secretaría" error={errors.secretariatName}>
          <InputText
            value={form.secretariatName}
            onChange={(e) => update({ secretariatName: e.target.value })}
            className="flit-field-input w-full"
            data-testid="reglas-contact-secretariat-name"
          />
        </FlitFormField>
        <FlitFormField label="Nombre contacto" error={errors.contactName}>
          <InputText
            value={form.contactName}
            onChange={(e) => update({ contactName: e.target.value })}
            className="flit-field-input w-full"
            data-testid="reglas-contact-name"
          />
        </FlitFormField>
        <FlitFormField label="Correo" error={errors.contactEmail}>
          <InputText
            type="email"
            value={form.contactEmail}
            onChange={(e) => update({ contactEmail: e.target.value })}
            className="flit-field-input w-full"
            data-testid="reglas-contact-email"
          />
        </FlitFormField>
        <FlitFormField label="Teléfono">
          <InputText
            value={form.contactPhone}
            onChange={(e) => update({ contactPhone: e.target.value })}
            className="flit-field-input w-full"
          />
        </FlitFormField>
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
    </FlitModal>
  );
}
