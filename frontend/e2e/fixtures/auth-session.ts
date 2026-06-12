import type { Page } from "@playwright/test";

export const E2E_TENANT_ID = "22222222-2222-2222-2222-222222222222";

export async function installAuthSession(page: Page) {
  await page.addInitScript((tenantId: string) => {
    window.localStorage.setItem(
      "gdc-auth-session",
      JSON.stringify({
        accessToken: "e2e-token",
        refreshToken: "e2e-refresh",
        userId: "33333333-3333-4333-8333-333333333303",
        tenantId,
        email: "superadmin@example.com",
        role: "SuperAdmin",
        expiresAt: "2099-01-01T00:00:00Z",
      }),
    );
  }, E2E_TENANT_ID);
}
