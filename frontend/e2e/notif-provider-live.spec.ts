import { expect, test } from "@playwright/test";

const API_BASE = process.env.PLAYWRIGHT_API_BASE_URL ?? "http://localhost:40303";
const LIVE = process.env.LIVE_NOTIF_TEST === "1";

test.describe("NOTIF proveedor — prueba SMTP real", () => {
  test.skip(!LIVE, "Defina LIVE_NOTIF_TEST=1 y levante core-api en :40303");

  test.beforeEach(async ({ page }) => {
    const loginRes = await fetch(`${API_BASE}/api/v1/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        email: "superadmin@example.com",
        password: "Str0ng!Pass",
      }),
    });

    if (!loginRes.ok) {
      throw new Error(`Login dev falló (${loginRes.status}). ¿core-api en ${API_BASE}?`);
    }

    const session = (await loginRes.json()) as {
      accessToken: string;
      userId: string;
      tenantId: string;
      email: string;
      role: string;
      expiresAt: string;
    };

    await page.addInitScript((stored) => {
      window.localStorage.setItem("gdc-auth-session", JSON.stringify(stored));
    }, session);
  });

  test("Admin NOTIF — precarga SMTP y envía correo de prueba", async ({ page }) => {
    await page.goto("/admin?tab=notificaciones", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("notif-provider-form")).toBeVisible({ timeout: 15_000 });

    await expect(page.locator("#smtp-host")).toHaveValue("smtp.office365.com", { timeout: 10_000 });
    await expect(page.locator("#provider-from")).toHaveValue("tramitesvehiculos@flitsas.com");
    await expect(page.getByTestId("provider-test-destino")).toHaveValue("willyn.londono@flitsas.com");

    await page.getByTestId("provider-test-btn").click();
    await expect(page.getByRole("status")).toContainText(/sent|enviado|Test email/i, {
      timeout: 90_000,
    });
  });
});
