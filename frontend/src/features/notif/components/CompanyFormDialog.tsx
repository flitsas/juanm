"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { InputMask } from "primereact/inputmask";
import { InputText } from "primereact/inputtext";
import { useEffect, useState } from "react";
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

  if (!open) return null;

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
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="company-form-title"
      data-testid="notif-company-form-dialog"
    >
      <div className="w-full max-w-lg rounded-2xl bg-[var(--card)] p-6 shadow-xl">
        <div className="flex items-center justify-between gap-4">
          <h2 id="company-form-title" className="text-xl font-bold text-[var(--deep)]">
            {company ? "Editar compañía" : "Nueva compañía"}
          </h2>
          <button
            type="button"
            onClick={() => onOpenChange(false)}
            className="rounded-full px-3 py-1 text-sm text-[var(--muted-foreground)] hover:bg-[var(--muted)]"
            aria-label="Cerrar diálogo"
          >
            Cerrar
          </button>
        </div>

        <div className="mt-6 space-y-4">
          <div className="flex flex-col gap-1">
            <label htmlFor="company-name" className="text-xs text-[var(--muted-foreground)]">
              Nombre
            </label>
            <InputText
              id="company-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className={`flit-field-input w-full ${errors.name ? "p-invalid" : ""}`}
              aria-invalid={Boolean(errors.name)}
            />
            {errors.name ? (
              <span className="text-xs text-[var(--alert)]" role="alert">
                {errors.name}
              </span>
            ) : null}
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="company-nit" className="text-xs text-[var(--muted-foreground)]">
              NIT
            </label>
            <InputText
              id="company-nit"
              value={nit}
              keyfilter="int"
              onChange={(e) => setNit(e.target.value)}
              className="flit-field-input w-full"
              placeholder="Solo números"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="company-email" className="text-xs text-[var(--muted-foreground)]">
              Correo de contacto
            </label>
            <InputText
              id="company-email"
              type="email"
              value={contactEmail}
              onChange={(e) => setContactEmail(e.target.value)}
              className={`flit-field-input w-full ${errors.contactEmail ? "p-invalid" : ""}`}
              aria-invalid={Boolean(errors.contactEmail)}
            />
            {errors.contactEmail ? (
              <span className="text-xs text-[var(--alert)]" role="alert">
                {errors.contactEmail}
              </span>
            ) : null}
          </div>

          <div className="flex flex-col gap-1">
            <label htmlFor="company-phone" className="text-xs text-[var(--muted-foreground)]">
              Teléfono
            </label>
            <InputMask
              id="company-phone"
              mask="9999999999"
              value={contactPhone}
              onChange={(e) => setContactPhone(e.value ?? "")}
              className={`flit-field-input w-full ${errors.contactPhone ? "p-invalid" : ""}`}
              placeholder="3001234567"
              aria-invalid={Boolean(errors.contactPhone)}
            />
            {errors.contactPhone ? (
              <span className="text-xs text-[var(--alert)]" role="alert">
                {errors.contactPhone}
              </span>
            ) : null}
          </div>

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

        <div className="mt-6 flex flex-wrap items-center justify-between gap-3">
          {company && onDelete ? (
            <Button
              type="button"
              label="Eliminar compañía"
              className="flit-btn-danger"
              loading={deleting}
              onClick={onDelete}
              data-testid={`delete-company-${company.id}`}
            />
          ) : (
            <span />
          )}
          <div className="flex gap-3">
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
          </div>
        </div>
      </div>
    </div>
  );
}
