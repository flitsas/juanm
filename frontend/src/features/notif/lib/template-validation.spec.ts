import { describe, expect, it } from "vitest";
import {
  EMPTY_TEMPLATE_FORM,
  hasTemplateErrors,
  validateTemplateForm,
} from "./template-validation";

describe("validateTemplateForm", () => {
  it("rechaza guardar sin cuerpo de mensaje", () => {
    const errors = validateTemplateForm({
      ...EMPTY_TEMPLATE_FORM,
      name: "Notificación",
      subject: "Asunto",
      htmlBody: "   ",
    });
    expect(hasTemplateErrors(errors)).toBe(true);
    expect(errors.htmlBody).toBeTruthy();
  });

  it("pasa validación con campos mínimos", () => {
    const errors = validateTemplateForm({
      ...EMPTY_TEMPLATE_FORM,
      name: "Notificación",
      subject: "Comparendo {{numero_comparendo}}",
      htmlBody: "<p>Hola {{infractor}}</p>",
    });
    expect(hasTemplateErrors(errors)).toBe(false);
  });
});
