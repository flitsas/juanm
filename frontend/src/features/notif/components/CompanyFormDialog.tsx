"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { InputMask } from "primereact/inputmask";
import { InputText } from "primereact/inputtext";
import { useEffect, useState } from "react";
import { FlitFormField, FlitModal, FlitModalActionsSplit } from "@/components/flit/modal-form";
import type { CreateCompanyPayload, NotifCompany } from "../lib/notif.types";

type CompanyFormDialogProps = {
  open: boolean;
  company: NotifCompany | null;
  saving?: boolean;
  deleting?: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (payload: CreateCompanyPayload & { isActive?: boolean }) => void;
  onDelete?: () => void;
};

type FieldErrors = Partial<Record<"name" | "contactEmail" | "contactPhone", string>>;

export function CompanyFormDialog({
  open,
  company,
  saving,
  deleting,
  onOpenChange,
  onSubmit,
  onDelete,
}: CompanyFormDialogProps) {
  const [name, setName] = useState("");
  const [nit, setNit] = useState("");
  const [contactPhone, setContactPhone] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [errors, setErrors] = useState<FieldErrors>({});

  useEffect(() => {
    if (!open) return;
    setName(company?.name ?? "");
    setNit(company?.nit ?? "");
    setContactPhone(company?.contactPhone ?? "");
    setContactEmail(company?.contactEmail ?? "");
    setIsActive(company?.isActive ?? true);
    setErrors({});
  }, [open, company]);

  const handleSubmit = () => {
    const nextErrors: FieldErrors = {};
    if (!name.trim()) {
      nextErrors.name = "El nombre es obligatorio.";
    }
    if (contactEmail && !contactEmail.includes("@")) {
      nextErrors.contactEmail = "Ingrese un correo válido.";
    }
    if (contactPhone && contactPhone.replace(/\D/g, "").length < 10) {
      nextErrors.contactPhone = "Ingrese un teléfono válido de 10 dígitos.";
    }
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0) return;

    onSubmit({
      name: name.trim(),
      nit: nit.trim() || undefined,
      contactPhone: contactPhone.trim() || undefined,
      contactEmail: contactEmail.trim() || undefined,
      isActive: company ? isActive : undefined,
    });
  };

  return (
    <FlitModal
      open={open}
      onClose={() => onOpenChange(false)}
      title={company ? "Editar compañía" : "Nueva compañía"}
      titleId="company-form-title"
      testId="notif-company-form-dialog"
      footer={
        <FlitModalActionsSplit
          start={
            company && onDelete ? (
              <Button
                type="button"
                label="Eliminar compañía"
                className="flit-btn-danger"
                loading={deleting}
                onClick={onDelete}
                data-testid={`delete-company-${company.id}`}
              />
            ) : undefined
          }
          end={
            <>
              <Button
                type="button"
                label="Cancelar"
                className="flit-btn-secondary"
                onClick={() => onOpenChange(false)}
              />
              <Button
                type="button"
                label={company ? "Guardar cambios" : "Crear compañía"}
                className="flit-btn-primary"
                loading={saving}
                onClick={handleSubmit}
                data-testid="company-form-submit"
              />
            </>
          }
        />
      }
    >
      <div className="space-y-4">
        <FlitFormField label="Nombre" htmlFor="company-name" error={errors.name}>
          <InputText
            id="company-name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className={`flit-field-input w-full ${errors.name ? "p-invalid" : ""}`}
            aria-invalid={Boolean(errors.name)}
          />
        </FlitFormField>

        <FlitFormField label="NIT" htmlFor="company-nit">
          <InputText
            id="company-nit"
            value={nit}
            keyfilter="int"
            onChange={(e) => setNit(e.target.value)}
            className="flit-field-input w-full"
            placeholder="Solo números"
          />
        </FlitFormField>

        <FlitFormField
          label="Correo de contacto"
          htmlFor="company-email"
          error={errors.contactEmail}
        >
          <InputText
            id="company-email"
            type="email"
            value={contactEmail}
            onChange={(e) => setContactEmail(e.target.value)}
            className={`flit-field-input w-full ${errors.contactEmail ? "p-invalid" : ""}`}
            aria-invalid={Boolean(errors.contactEmail)}
          />
        </FlitFormField>

        <FlitFormField label="Teléfono" htmlFor="company-phone" error={errors.contactPhone}>
          <InputMask
            id="company-phone"
            mask="9999999999"
            value={contactPhone}
            onChange={(e) => setContactPhone(e.value ?? "")}
            className={`flit-field-input w-full ${errors.contactPhone ? "p-invalid" : ""}`}
            placeholder="3001234567"
            aria-invalid={Boolean(errors.contactPhone)}
          />
        </FlitFormField>

        {company ? (
          <div className="flex items-center gap-2">
            <Checkbox
              inputId="company-active"
              checked={isActive}
              onChange={(e) => setIsActive(Boolean(e.checked))}
            />
            <label htmlFor="company-active" className="text-sm text-[var(--deep)]">
              Compañía activa
            </label>
          </div>
        ) : null}
      </div>
    </FlitModal>
  );
}
