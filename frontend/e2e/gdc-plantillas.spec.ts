import { expect, test } from "@playwright/test";
import { installAuthSession } from "./fixtures/auth-session";
import { installNotifApiMocks } from "./fixtures/notif-api-mock";
import { installPlantillasApiMocks, mockPdfTemplateSummary } from "./fixtures/plantillas-api-mock";

test.describe("GDC plantillas — Feature #9564", () => {
  test.beforeEach(async ({ page }) => {
    await installAuthSession(page);
    await installNotifApiMocks(page);
    await installPlantillasApiMocks(page);
  });

  test("Catálogo — plantillas activas en /gdc", async ({ page }) => {
    await page.goto("/gdc", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("gdc-plantillas-page")).toBeVisible({ timeout: 15_000 });
    await expect(page.getByTestId("gdc-templates-catalog")).toBeVisible();
    await expect(page.getByText(mockPdfTemplateSummary.name)).toBeVisible();
    await expect(page.getByTestId(`gdc-use-template-${mockPdfTemplateSummary.id}`)).toBeVisible();
  });
});
