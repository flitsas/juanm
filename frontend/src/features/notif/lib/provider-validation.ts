import type { ProviderType } from "./notif.types";

export type ProviderFormState = {
  providerType: ProviderType | "";
  fromAddress: string;
  testDestino: string;
  apiBaseUrl: string;
  apiKey: string;
  sendgridApiKey: string;
  smtpHost: string;
  smtpPort: string;
  smtpUsername: string;
  smtpPassword: string;
  smtpUseSsl: boolean;
};

export type ProviderFieldErrors = Partial<Record<keyof ProviderFormState, string>>;

export const EMPTY_PROVIDER_FORM: ProviderFormState = {
  providerType: "",
  fromAddress: "",
  testDestino: "",
  apiBaseUrl: "",
  apiKey: "",
  sendgridApiKey: "",
  smtpHost: "",
  smtpPort: "587",
  smtpUsername: "",
  smtpPassword: "",
  smtpUseSsl: true,
};

export function validateProviderForm(state: ProviderFormState): ProviderFieldErrors {
  const errors: ProviderFieldErrors = {};

  if (!state.providerType) {
    errors.providerType = "Seleccione un proveedor.";
  }

  if (!state.fromAddress.trim()) {
    errors.fromAddress = "El remitente es obligatorio.";
  } else if (!state.fromAddress.includes("@")) {
    errors.fromAddress = "Ingrese un correo remitente válido.";
  }

  if (state.providerType === "api") {
    if (!state.apiBaseUrl.trim()) {
      errors.apiBaseUrl = "La URL base es obligatoria.";
    }
  }

  if (state.providerType === "sendgrid" && !state.sendgridApiKey.trim()) {
    errors.sendgridApiKey = "La API Key de SendGrid es obligatoria.";
  }

  if (state.providerType === "flit_mail") {
    if (!state.smtpHost.trim()) {
      errors.smtpHost = "El host SMTP es obligatorio.";
    }
    if (!state.smtpUsername.trim()) {
      errors.smtpUsername = "El usuario SMTP es obligatorio.";
    }
    if (!state.smtpPassword.trim()) {
      errors.smtpPassword = "La contraseña SMTP es obligatoria.";
    }
  }

  return errors;
}

export function hasProviderErrors(errors: ProviderFieldErrors): boolean {
  return Object.keys(errors).length > 0;
}

export function buildProviderCredentials(state: ProviderFormState): Record<string, unknown> | null {
  switch (state.providerType) {
    case "api":
      return {
        baseUrl: state.apiBaseUrl.trim(),
        apiKey: state.apiKey.trim() || undefined,
      };
    case "sendgrid":
      return { apiKey: state.sendgridApiKey.trim() };
    case "flit_mail":
      return {
        host: state.smtpHost.trim(),
        port: Number.parseInt(state.smtpPort, 10) || 587,
        username: state.smtpUsername.trim(),
        password: state.smtpPassword,
        useSsl: state.smtpUseSsl,
      };
    default:
      return null;
  }
}
