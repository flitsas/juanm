import { expect, test } from "@playwright/test";
import { installAuthSession } from "./fixtures/auth-session";
import { installDgcApiMocks } from "./fixtures/dgc-api-mock";
import { installNotifApiMocks } from "./fixtures/notif-api-mock";
import { installPlantillasApiMocks } from "./fixtures/plantillas-api-mock";
import { installReglasApiMocks, mockReglasRule } from "./fixtures/reglas-api-mock";

test.describe("Vialix — 4 features conectados", () => {
  test.beforeEach(async ({ page }) => {
    await installAuthSession(page);
    await installNotifApiMocks(page);
    await installDgcApiMocks(page);
    await installPlantillasApiMocks(page);
    await installReglasApiMocks(page);
  });

  test("Navegación shell — DGC, Plantillas, Admin y Reglas DP", async ({ page }) => {
    await page.goto("/", { waitUntil: "domcontentloaded" });

    await page.getByTestId("dock-nav-dgc").click();
    await expect(page.getByTestId("dgc-maestra-page")).toBeVisible({ timeout: 15_000 });

    await page.getByTestId("dock-nav-gdc").click();
    await expect(page.getByTestId("gdc-plantillas-page")).toBeVisible();

    await page.getByTestId("dock-nav-reglas").click();
    await expect(page.getByTestId("reglas-page")).toBeVisible();
    await expect(page.getByText(mockReglasRule.name)).toBeVisible();

    await page.goto("/admin?tab=notificaciones", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("notif-admin-page")).toBeVisible();
    await expect(page.getByTestId("notif-provider-form")).toBeVisible();
  });
});
