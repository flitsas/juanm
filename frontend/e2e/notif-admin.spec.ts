import { expect, test } from "@playwright/test";
import { installNotifApiMocks, mockCompany, mockQueueItem } from "./fixtures/notif-api-mock";

test.describe("NOTIF admin — Feature #9563", () => {
  test.beforeEach(async ({ page }) => {
    await installNotifApiMocks(page);
    await page.goto("/admin/notificaciones", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("notif-admin-page")).toBeVisible();
  });

  test("RF01 — tab Compañías lista la nómina", async ({ page }) => {
    await expect(page.getByTestId("notif-tab-companies")).toBeVisible();
    const table = page.getByTestId("notif-companies-table");
    await expect(table).toBeVisible();
    await expect(table.getByRole("cell", { name: mockCompany.name })).toBeVisible();
    await expect(table.getByText(mockCompany.contactEmail ?? "")).toBeVisible();
  });

  test("RF02/RF03 — tab Proveedor muestra perfil tenant y formulario", async ({ page }) => {
    await page.getByTestId("notif-tab-provider").click();
    const profile = page.getByTestId("notif-tenant-profile");
    await expect(profile).toBeVisible();
    await expect(profile.getByText(mockCompany.name)).toBeVisible();
    await expect(page.getByTestId("notif-provider-form")).toBeVisible();
  });

  test("RF02 — guardar perfil tenant actualiza contacto", async ({ page }) => {
    await page.getByTestId("notif-tab-provider").click();
    await expect(page.getByTestId("notif-tenant-profile")).toBeVisible();
    const emailInput = page.locator("#profile-email");
    await emailInput.fill("nuevo@flit.dev");
    await page.getByTestId("profile-save-btn").click();
    await expect(page.getByText("Perfil actualizado correctamente.")).toBeVisible();
    await expect(emailInput).toHaveValue("nuevo@flit.dev");
  });

  test("RF04 — tab Plantillas carga listado", async ({ page }) => {
    await page.getByTestId("notif-tab-templates").click();
    const section = page.getByTestId("notif-templates-section");
    await expect(section).toBeVisible();
    await expect(section.getByText("Aviso comparendo")).toBeVisible();
  });

  test("RF05–RF08 — tab Reglas muestra switch, reglas y cola", async ({ page }) => {
    await page.getByTestId("notif-tab-rules").click();
    await expect(page.getByTestId("notif-rules-section")).toBeVisible();
    await expect(page.getByTestId("notif-dispatch-switch")).toBeVisible();
    await expect(page.getByTestId("notif-rules-table")).toBeVisible();
    await expect(page.getByText("Recordatorio 5 días")).toBeVisible();
    const queue = page.getByTestId("notif-queue-table");
    await expect(queue).toBeVisible();
    await expect(queue.getByText(mockQueueItem.destino)).toBeVisible();
    await expect(queue.getByRole("cell", { name: "Pendiente" })).toBeVisible();
  });

  test("RF08 — toggle switch operativo On/Off", async ({ page }) => {
    await page.getByTestId("notif-tab-rules").click();
    await expect(page.getByTestId("notif-switch-status")).toHaveText(/On/);
    await page.getByTestId("notif-switch-toggle").click();
    await expect(page.getByText(/Switch desactivado/)).toBeVisible();
    await expect(page.getByTestId("notif-switch-status")).toHaveText(/Off/);
  });
});
