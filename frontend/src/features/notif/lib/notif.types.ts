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

export type TenantProfile = {
  tenantId: string;
  name: string;
  contactPhone: string | null;
  contactEmail: string | null;
};

export type UpdateTenantProfilePayload = {
  contactPhone?: string;
  contactEmail?: string;
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

/** Solo Development — precarga desde appsettings.Development.local.json vía API. */
export type DevProviderPrefill = {
  providerType: ProviderType;
  fromAddress: string;
  smtpHost: string;
  smtpPort: number;
  smtpUsername: string;
  smtpPassword: string;
  smtpUseSsl: boolean;
  testDestino: string;
};

export type NotifTemplate = {
  id: string;
  name: string;
  subject: string;
  htmlBody: string;
  bannerUrl: string | null;
  footerUrl: string | null;
  createdAt: string;
  updatedAt: string | null;
};

export type NotifTemplateListResult = {
  items: NotifTemplate[];
};

export type SaveTemplatePayload = {
  name: string;
  subject: string;
  htmlBody: string;
  bannerUrl?: string | null;
  footerUrl?: string | null;
};

export type PreviewTemplateResult = {
  subject: string;
  htmlBody: string;
};

export type UploadTemplateAssetResult = {
  url: string;
  contentType: string;
};

export type RuleTriggerType = "chronological" | "state";

export type TriggerReference = "fecha_comparendo" | "fecha_notificacion";

export type NotifRule = {
  id: string;
  emailTemplateId: string;
  name: string;
  triggerType: RuleTriggerType;
  triggerDays: number | null;
  triggerReference: TriggerReference | null;
  triggerEstado: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
};

export type NotifRuleListResult = {
  items: NotifRule[];
};

export type SaveRulePayload = {
  emailTemplateId: string;
  name: string;
  triggerType: RuleTriggerType;
  triggerDays?: number | null;
  triggerReference?: TriggerReference | null;
  triggerEstado?: string | null;
  isActive?: boolean;
};

export type NotifQueueItem = {
  id: string;
  notificationRuleId: string;
  emailTemplateId: string;
  comparendoId: string;
  destino: string;
  status: string;
  scheduledAt: string;
  processedAt: string | null;
  errorMessage: string | null;
};

export type NotifQueueListResult = {
  items: NotifQueueItem[];
};

export type NotifSwitchState = {
  dispatchEnabled: boolean;
};
