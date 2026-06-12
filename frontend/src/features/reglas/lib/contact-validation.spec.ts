import { describe, expect, it } from "vitest";
import { EMPTY_CONTACT_FORM, hasContactErrors, validateContactForm } from "./contact-validation";

describe("validateContactForm", () => {
  it("exige campos obligatorios y email válido", () => {
    const errors = validateContactForm(EMPTY_CONTACT_FORM);
    expect(hasContactErrors(errors)).toBe(true);
    expect(errors.secretariatCode).toBeTruthy();
    expect(errors.contactEmail).toBeTruthy();
  });

  it("acepta contacto completo", () => {
    const errors = validateContactForm({
      ...EMPTY_CONTACT_FORM,
      secretariatCode: "BOG",
      secretariatName: "Bogotá",
      contactName: "Operador",
      contactEmail: "sec@example.com",
    });
    expect(hasContactErrors(errors)).toBe(false);
  });
});
