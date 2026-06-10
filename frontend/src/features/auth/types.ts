export type AuthSession = {
  accessToken: string;
  expiresAt: string;
  userId: string;
  tenantId: string;
  email: string;
  role: string;
};

export type LoginPayload = {
  email: string;
  password: string;
};
