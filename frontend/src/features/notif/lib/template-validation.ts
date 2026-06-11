export type TemplateFormState = {
  name: string;
  subject: string;
  htmlBody: string;
  bannerUrl: string;
  footerUrl: string;
};

export type TemplateFieldErrors = Partial<Record<keyof TemplateFormState, string>>;

export const EMPTY_TEMPLATE_FORM: TemplateFormState = {
  name: "",
  subject: "",
  htmlBody: "",
  bannerUrl: "",
  footerUrl: "",
};

export function validateTemplateForm(state: TemplateFormState): TemplateFieldErrors {
  const errors: TemplateFieldErrors = {};

  if (!state.name.trim()) {
    errors.name = "El nombre es obligatorio.";
  }

  if (!state.subject.trim()) {
    errors.subject = "El asunto es obligatorio.";
  }

  if (!state.htmlBody.trim()) {
    errors.htmlBody = "El cuerpo del mensaje es obligatorio.";
  }

  return errors;
}

export function hasTemplateErrors(errors: TemplateFieldErrors): boolean {
  return Object.keys(errors).length > 0;
}
