import { expect, test } from "@playwright/test";
import { installAuthSession } from "./fixtures/auth-session";
import { installNotifApiMocks } from "./fixtures/notif-api-mock";
import { installPlantillasApiMocks } from "./fixtures/plantillas-api-mock";
import {
  installReglasApiMocks,
  mockReglasContact,
  mockReglasRule,
  mockReglasRun,
} from "./fixtures/reglas-api-mock";

test.describe("REGLAS admin — Feature #9710 smoke", () => {
  test.beforeEach(async ({ page }) => {
    await installAuthSession(page);
    await installNotifApiMocks(page);
    await installPlantillasApiMocks(page);
    await installReglasApiMocks(page);
  });

  test("Constructor — lista reglas en /admin/reglas", async ({ page }) => {
    await page.goto("/admin/reglas", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("reglas-page")).toBeVisible({ timeout: 15_000 });
    await expect(page.getByTestId("reglas-rules-table")).toBeVisible();
    await expect(page.getByText(mockReglasRule.name)).toBeVisible();
  });

  test("Ejecución — scheduler, corridas y coincidencias", async ({ page }) => {
    await page.goto("/admin/reglas?tab=ejecucion", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("reglas-execution-section")).toBeVisible({ timeout: 15_000 });
    await expect(page.getByTestId("reglas-scheduler-panel")).toBeVisible();
    const runsTable = page.getByTestId("reglas-runs-table");
    await expect(runsTable).toBeVisible();
    await expect(runsTable.getByText(`${mockReglasRun.evaluatedCount} eval.`)).toBeVisible();
    await expect(page.getByTestId("reglas-matches-table")).toBeVisible();
  });

  test("Logs y contactos — métricas y CRUD contacto", async ({ page }) => {
    await page.goto("/admin/reglas?tab=logs", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("reglas-logs-contacts-section")).toBeVisible({ timeout: 15_000 });
    await expect(page.getByTestId("reglas-logs-metrics")).toBeVisible();
    const contactsTable = page.getByTestId("reglas-contacts-table");
    await expect(contactsTable).toBeVisible();
    await expect(contactsTable.getByText(mockReglasContact.secretariatName)).toBeVisible();

    await page.getByTestId("reglas-new-contact-btn").click();
    await expect(page.getByTestId("reglas-contact-form-dialog")).toBeVisible();
    await page.getByTestId("reglas-contact-code").fill("MED");
    await page.getByTestId("reglas-contact-secretariat-name").fill("Medellín");
    await page.getByTestId("reglas-contact-name").fill("Operador Med");
    await page.getByTestId("reglas-contact-email").fill("medellin@flit.dev");
    await page.getByTestId("reglas-contact-save-btn").click();
    await expect(page.getByText("medellin@flit.dev")).toBeVisible();
  });
});
