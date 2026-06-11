import type { Page, Route } from "@playwright/test";

const TENANT_ID = "22222222-2222-2222-2222-222222222222";

export const mockCompany = {
  id: "11111111-1111-1111-1111-111111111111",
  name: "Flit Renting E2E",
  nit: "900123456",
  contactPhone: "3001234567",
  contactEmail: "ops@flit.dev",
  isActive: true,
  createdAt: "2026-06-01T00:00:00Z",
  updatedAt: null,
};

export const mockProfile = {
  tenantId: TENANT_ID,
  name: "Flit Renting E2E",
  contactPhone: "3001234567",
  contactEmail: "ops@flit.dev",
};

export const mockProvider = {
  id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  providerType: "sendgrid",
  fromAddress: "noreply@flit.dev",
  isActive: true,
  dispatchEnabled: true,
  hasCredentials: true,
  updatedAt: "2026-06-01T00:00:00Z",
};

export const mockTemplate = {
  id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  name: "Aviso comparendo",
  subject: "Comparendo {{placa}}",
  htmlBody: "<p>Hola {{nombre}}</p>",
  bannerUrl: null,
  footerUrl: null,
  createdAt: "2026-06-01T00:00:00Z",
  updatedAt: null,
};

export const mockRule = {
  id: "cccccccc-cccc-cccc-cccc-cccccccccccc",
  emailTemplateId: mockTemplate.id,
  name: "Recordatorio 5 días",
  triggerType: "chronological",
  triggerDays: 5,
  triggerReference: "fecha_comparendo",
  triggerEstado: null,
  isActive: true,
  createdAt: "2026-06-01T00:00:00Z",
  updatedAt: null,
};

export const mockQueueItem = {
  id: "dddddddd-dddd-dddd-dddd-dddddddddddd",
  notificationRuleId: mockRule.id,
  emailTemplateId: mockTemplate.id,
  comparendoId: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  destino: "conductor@flit.dev",
  status: "pending",
  scheduledAt: "2026-06-10T12:00:00Z",
  processedAt: null,
  errorMessage: null,
};

let profileState = { ...mockProfile };
let switchState = { dispatchEnabled: true };

export function resetNotifMocks() {
  profileState = { ...mockProfile };
  switchState = { dispatchEnabled: true };
}

function notifPath(url: string): string {
  const pathname = new URL(url).pathname;
  const idx = pathname.indexOf("/api/v1/notif");
  return idx >= 0 ? pathname.slice(idx + "/api/v1/notif".length) : pathname;
}

export async function installNotifApiMocks(page: Page) {
  resetNotifMocks();

  await page.route("**/api/v1/notif/**", async (route: Route) => {
    const path = notifPath(route.request().url());
    const method = route.request().method();

    if (path === "/companies" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: [mockCompany] } });
    }

    if (path === "/profile" && method === "GET") {
      return route.fulfill({ status: 200, json: profileState });
    }

    if (path === "/profile" && method === "PUT") {
      const body = route.request().postDataJSON() as {
        contactPhone?: string;
        contactEmail?: string;
      };
      profileState = {
        ...profileState,
        contactPhone: body.contactPhone ?? profileState.contactPhone,
        contactEmail: body.contactEmail ?? profileState.contactEmail,
      };
      return route.fulfill({ status: 200, json: profileState });
    }

    if (path === "/provider" && method === "GET") {
      return route.fulfill({ status: 200, json: mockProvider });
    }

    if (path === "/templates" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: [mockTemplate] } });
    }

    if (path === "/rules" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: [mockRule] } });
    }

    if (path === "/queue" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: [mockQueueItem] } });
    }

    if (path === "/switch" && method === "GET") {
      return route.fulfill({ status: 200, json: switchState });
    }

    if (path === "/switch" && method === "PUT") {
      const body = route.request().postDataJSON() as { dispatchEnabled: boolean };
      switchState = { dispatchEnabled: body.dispatchEnabled };
      return route.fulfill({ status: 200, json: switchState });
    }

    return route.fulfill({
      status: 404,
      json: { message: `E2E mock missing: ${method} ${path}` },
    });
  });
}
