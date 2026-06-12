import { expect, test } from "@playwright/test";
import { installAuthSession } from "./fixtures/auth-session";
import { installDgcApiMocks, mockComparendo } from "./fixtures/dgc-api-mock";
import { installNotifApiMocks } from "./fixtures/notif-api-mock";
import { installPlantillasApiMocks } from "./fixtures/plantillas-api-mock";

test.describe("DGC maestra — Feature #9560", () => {
  test.beforeEach(async ({ page }) => {
    await installAuthSession(page);
    await installNotifApiMocks(page);
    await installDgcApiMocks(page);
    await installPlantillasApiMocks(page);
  });

  test("Maestra — lista comparendos en /dgc", async ({ page }) => {
    await page.goto("/dgc", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("dgc-maestra-page")).toBeVisible({ timeout: 15_000 });
    await expect(page.getByTestId("dgc-maestra-table")).toBeVisible();
    await expect(page.getByText(mockComparendo.numeroComparendo)).toBeVisible();
  });

  test("Detalle — pestañas detalle, DP y correos", async ({ page }) => {
    await page.goto("/dgc", { waitUntil: "domcontentloaded" });
    await page
      .getByRole("button", { name: `Abrir detalle ${mockComparendo.numeroComparendo}` })
      .click();
    await expect(page.getByTestId("dgc-detail-panel")).toBeVisible();
    await expect(page.getByTestId("gdc-dp-section")).toBeVisible();
    await expect(page.getByTestId("gdc-dp-grid")).toBeVisible();
    await page.getByRole("tab", { name: "Log de correos" }).click();
    await expect(page.getByTestId("dgc-detail-tab-correos")).toBeVisible();
  });
});
