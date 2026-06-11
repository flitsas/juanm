import { describe, expect, it } from "vitest";
import {
  EMPTY_PROVIDER_FORM,
  hasProviderErrors,
  validateProviderForm,
} from "./provider-validation";

describe("validateProviderForm", () => {
  it("rechaza guardar sin proveedor ni remitente", () => {
    const errors = validateProviderForm(EMPTY_PROVIDER_FORM);
    expect(hasProviderErrors(errors)).toBe(true);
    expect(errors.providerType).toBeTruthy();
    expect(errors.fromAddress).toBeTruthy();
  });

  it("exige API Key para SendGrid", () => {
    const errors = validateProviderForm({
      ...EMPTY_PROVIDER_FORM,
      providerType: "sendgrid",
      fromAddress: "notif@flit.dev",
      sendgridApiKey: "",
    });
    expect(errors.sendgridApiKey).toBeTruthy();
  });

  it("pasa validación mínima para API REST", () => {
    const errors = validateProviderForm({
      ...EMPTY_PROVIDER_FORM,
      providerType: "api",
      fromAddress: "notif@flit.dev",
      apiBaseUrl: "https://mail.test",
    });
    expect(hasProviderErrors(errors)).toBe(false);
  });
});
