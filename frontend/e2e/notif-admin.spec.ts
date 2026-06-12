import { expect, test } from "@playwright/test";
import { installAuthSession } from "./fixtures/auth-session";
import { installNotifApiMocks, mockCompany, mockQueueItem } from "./fixtures/notif-api-mock";

test.describe("NOTIF admin — Feature #9563", () => {
  test.beforeEach(async ({ page }) => {
    await installAuthSession(page);
    await installNotifApiMocks(page);
  });

  test("RF01 — panel Compañías lista la nómina en /admin", async ({ page }) => {
    await page.goto("/admin", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("unified-admin-page")).toBeVisible();
    await expect(page.getByTestId("admin-companies-sidebar")).toBeVisible();
    await expect(page.getByTestId(`company-card-${mockCompany.id}`)).toBeVisible();
    await expect(
      page.getByTestId(`company-card-${mockCompany.id}`).getByText(mockCompany.name),
    ).toBeVisible();
    await page.getByTestId(`edit-company-${mockCompany.id}`).click();
    await expect(page.getByTestId("notif-company-form-dialog")).toBeVisible();
    await expect(page.locator("#company-email")).toHaveValue(mockCompany.contactEmail ?? "");
  });

  test("RF02/RF03 — tab Proveedor muestra perfil tenant y formulario", async ({ page }) => {
    await page.goto("/admin?tab=notificaciones", { waitUntil: "domcontentloaded" });
    await expect(page.getByTestId("notif-admin-page")).toBeVisible();
    await page.getByTestId("notif-tab-provider").click();
    const profile = page.getByTestId("notif-tenant-profile");
    await expect(profile).toBeVisible();
    await expect(profile.getByText(mockCompany.name)).toBeVisible();
    await expect(page.getByTestId("notif-provider-form")).toBeVisible();
  });

  test("RF02 — guardar perfil tenant actualiza contacto", async ({ page }) => {
    await page.goto("/admin?tab=notificaciones", { waitUntil: "domcontentloaded" });
    await page.getByTestId("notif-tab-provider").click();
    await expect(page.getByTestId("notif-tenant-profile")).toBeVisible();
    const emailInput = page.locator("#profile-email");
    await emailInput.fill("nuevo@flit.dev");
    await page.getByTestId("profile-save-btn").click();
    await expect(page.getByText("Perfil actualizado correctamente.")).toBeVisible();
    await expect(emailInput).toHaveValue("nuevo@flit.dev");
  });

  test("RF04 — tab Plantillas carga listado", async ({ page }) => {
    await page.goto("/admin?tab=notificaciones", { waitUntil: "domcontentloaded" });
    await page.getByTestId("notif-tab-templates").click();
    const section = page.getByTestId("notif-templates-section");
    await expect(section).toBeVisible();
    await expect(section.getByText("Aviso comparendo")).toBeVisible();
  });

  test("RF05–RF08 — tab Reglas muestra switch, reglas y cola", async ({ page }) => {
    await page.goto("/admin?tab=notificaciones", { waitUntil: "domcontentloaded" });
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
    await page.goto("/admin?tab=notificaciones", { waitUntil: "domcontentloaded" });
    await page.getByTestId("notif-tab-rules").click();
    await expect(page.getByTestId("notif-switch-status")).toHaveText(/On/);
    await page.getByTestId("notif-switch-toggle").click();
    await expect(page.getByText(/Switch desactivado/)).toBeVisible();
    await expect(page.getByTestId("notif-switch-status")).toHaveText(/Off/);
  });
});
