import type { Page, Route } from "@playwright/test";

export const mockComparendo = {
  id: "dddddddd-dddd-dddd-dddd-dddddddddddd",
  numeroComparendo: "CMP-E2E-001",
  estado: "Notificado",
  placa: "ABC123",
  infractor: "Juan Pérez",
  documento: "1234567890",
  infraccion: "C29",
  secretaria: "Bogotá",
  fechaComparendo: "2026-06-01",
  fechaNotificacion: "2026-06-05",
  diasRestantes: 12,
  total: 450000,
  pago: "Pendiente",
  fuente: "OCR",
  contraventor: "Juan Pérez",
  tieneContraventor: true,
};

export async function installDgcApiMocks(page: Page) {
  await page.route("**/api/v1/dgc/**", async (route: Route) => {
    const url = new URL(route.request().url());
    const path = url.pathname.replace(/^.*\/api\/v1\/dgc/, "");
    const method = route.request().method();

    if (path === "/comparendos" && method === "GET") {
      return route.fulfill({
        status: 200,
        json: {
          items: [mockComparendo],
          totalCount: 1,
          totalPages: 1,
          page: 1,
          pageSize: 25,
        },
      });
    }

    const emailsMatch = path.match(/^\/comparendos\/([^/]+)\/emails$/);
    if (emailsMatch && method === "GET") {
      return route.fulfill({
        status: 200,
        json: {
          items: [
            {
              id: "email-1",
              sentAt: "2026-06-06T10:00:00Z",
              origen: "notif@flit.dev",
              destino: "conductor@flit.dev",
              cc: null,
              tipoAlerta: "Aviso comparendo",
              estadoEntrega: "Entregado",
            },
          ],
        },
      });
    }

    await route.fallback();
  });
}
