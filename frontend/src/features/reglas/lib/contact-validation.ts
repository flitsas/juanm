import type { SaveReglasContactPayload } from "./reglas.types";

export type ContactFormState = {
  secretariatCode: string;
  secretariatName: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string;
  isActive: boolean;
};

export const EMPTY_CONTACT_FORM: ContactFormState = {
  secretariatCode: "",
  secretariatName: "",
  contactName: "",
  contactEmail: "",
  contactPhone: "",
  isActive: true,
};

export type ContactFormErrors = Partial<Record<keyof ContactFormState, string>>;

export function hasContactErrors(errors: ContactFormErrors): boolean {
  return Object.keys(errors).length > 0;
}

export function validateContactForm(form: ContactFormState): ContactFormErrors {
  const errors: ContactFormErrors = {};
  if (!form.secretariatCode.trim()) errors.secretariatCode = "Código de secretaría obligatorio.";
  if (!form.secretariatName.trim()) errors.secretariatName = "Nombre de secretaría obligatorio.";
  if (!form.contactName.trim()) errors.contactName = "Nombre de contacto obligatorio.";
  if (!form.contactEmail.trim()) {
    errors.contactEmail = "Correo obligatorio.";
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.contactEmail.trim())) {
    errors.contactEmail = "Correo inválido.";
  }
  return errors;
}

export function toContactPayload(form: ContactFormState): SaveReglasContactPayload {
  return {
    secretariatCode: form.secretariatCode.trim(),
    secretariatName: form.secretariatName.trim(),
    contactName: form.contactName.trim(),
    contactEmail: form.contactEmail.trim(),
    contactPhone: form.contactPhone.trim() || null,
    isActive: form.isActive,
  };
}
