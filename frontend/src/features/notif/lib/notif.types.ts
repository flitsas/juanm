export type NotifCompany = {
  id: string;
  name: string;
  nit: string | null;
  contactPhone: string | null;
  contactEmail: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
};

export type NotifCompanyListResult = {
  items: NotifCompany[];
};

export type CreateCompanyPayload = {
  name: string;
  nit?: string;
  contactPhone?: string;
  contactEmail?: string;
};

export type UpdateCompanyPayload = CreateCompanyPayload & {
  isActive?: boolean;
};

export type NotifProvider = {
  id: string;
  providerType: string;
  fromAddress: string;
  isActive: boolean;
  dispatchEnabled: boolean;
  hasCredentials: boolean;
  updatedAt: string | null;
};

export type SaveProviderPayload = {
  providerType: string;
  fromAddress: string;
  credentials: Record<string, unknown>;
};

export type TestProviderPayload = {
  destino: string;
  providerType?: string;
  fromAddress?: string;
  credentials?: Record<string, unknown>;
};

export type TestProviderResult = {
  success: boolean;
  message: string;
};

export type ProviderType = "api" | "sendgrid" | "flit_mail";
