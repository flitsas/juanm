import type { Page, Route } from "@playwright/test";

export const mockPdfTemplateSummary = {
  id: "33333333-3333-3333-3333-333333333333",
  name: "DP Vialix E2E",
  version: 1,
  isActive: true,
  fieldCount: 13,
};

const mockDpItem = {
  id: "dp-1",
  comparendoId: "dddddddd-dddd-dddd-dddd-dddddddddddd",
  templateId: mockPdfTemplateSummary.id,
  templateVersion: 1,
  estado: "generado",
  generatedAt: "2026-06-07T10:00:00Z",
  downloadPath: "/api/v1/gdc/derechos-peticion/dp-1/download",
};

export async function installPlantillasApiMocks(page: Page) {
  await page.route("**/api/v1/gdc/**", async (route: Route) => {
    const url = new URL(route.request().url());
    const path = url.pathname;
    const method = route.request().method();

    if (method === "GET" && path.endsWith("/api/v1/gdc/templates")) {
      return route.fulfill({
        status: 200,
        json: { items: [mockPdfTemplateSummary] },
      });
    }

    const dpsMatch = path.match(/\/api\/v1\/gdc\/comparendos\/([^/]+)\/derechos-peticion$/);
    if (dpsMatch && method === "GET") {
      return route.fulfill({
        status: 200,
        json: {
          items: [{ ...mockDpItem, comparendoId: dpsMatch[1] }],
        },
      });
    }

    await route.fallback();
  });
}
