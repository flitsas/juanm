import type { Page, Route } from "@playwright/test";

export const mockReglasRule = {
  id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  name: "DP Secretaría Bogotá",
  description: "Regla piloto E2E",
  isActive: true,
  pdfTemplateId: "33333333-3333-3333-3333-333333333333",
  emailSubject: "DP {{numero_comparendo}}",
  emailBodyHtml: "<p>Adjunto DP</p>",
  secretariatContactId: null,
  conditionRoot: {
    nodeType: "group",
    logicOperator: "and",
    children: [
      {
        nodeType: "predicate",
        fieldKey: "estado",
        comparisonOperator: "eq",
        comparisonValue: "Notificado",
      },
    ],
  },
  createdAt: "2026-06-12T10:00:00Z",
  updatedAt: null,
};

export const mockReglasRun = {
  id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  startedAt: "2026-06-12T10:00:00Z",
  finishedAt: "2026-06-12T10:00:05Z",
  status: "completed",
  triggerType: "manual",
  evaluatedCount: 12,
  matchedCount: 1,
  processedCount: 1,
  failedCount: 0,
  errorMessage: null,
};

export const mockReglasMatch = {
  id: "cccccccc-cccc-cccc-cccc-cccccccccccc",
  dynamicRuleId: mockReglasRule.id,
  ruleName: mockReglasRule.name,
  comparendoId: "dddddddd-dddd-dddd-dddd-dddddddddddd",
  comparendoNumero: "CMP-E2E-001",
  ruleExecutionRunId: mockReglasRun.id,
  status: "success",
  processedAt: "2026-06-12T10:00:06Z",
  createdAt: "2026-06-12T10:00:04Z",
};

export const mockReglasContact = {
  id: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  secretariatCode: "BOG",
  secretariatName: "Bogotá",
  contactName: "Operador E2E",
  contactEmail: "secretaria@flit.dev",
  contactPhone: "3001234567",
  isActive: true,
  createdAt: "2026-06-12T09:00:00Z",
  updatedAt: null,
};

let contactsState = [mockReglasContact];

export function resetReglasMocks() {
  contactsState = [mockReglasContact];
}

function reglasPath(url: string): string {
  const pathname = new URL(url).pathname;
  const idx = pathname.indexOf("/api/v1/reglas");
  return idx >= 0 ? pathname.slice(idx + "/api/v1/reglas".length) : pathname;
}

export async function installReglasApiMocks(page: Page) {
  resetReglasMocks();

  await page.route("**/api/v1/reglas/**", async (route: Route) => {
    const path = reglasPath(route.request().url());
    const method = route.request().method();

    if (path === "/rules" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: [mockReglasRule] } });
    }

    if (path === "/runs" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: [mockReglasRun] } });
    }

    if (path === "/runs" && method === "POST") {
      return route.fulfill({
        status: 202,
        json: { runId: mockReglasRun.id, matchedCount: 1 },
      });
    }

    if (path === "/matches" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: [mockReglasMatch] } });
    }

    if (path === "/contacts" && method === "GET") {
      return route.fulfill({ status: 200, json: { items: contactsState } });
    }

    if (path === "/contacts" && method === "POST") {
      const body = route.request().postDataJSON() as Omit<
        typeof mockReglasContact,
        "id" | "createdAt"
      >;
      const created = {
        ...mockReglasContact,
        ...body,
        id: "ffffffff-ffff-ffff-ffff-ffffffffffff",
        createdAt: new Date().toISOString(),
      };
      contactsState = [...contactsState, created];
      return route.fulfill({ status: 201, json: created });
    }

    const contactPut = path.match(/^\/contacts\/([^/]+)$/);
    if (contactPut && method === "PUT") {
      const id = contactPut[1];
      const body = route.request().postDataJSON() as typeof mockReglasContact;
      contactsState = contactsState.map((c) => (c.id === id ? { ...c, ...body, id } : c));
      return route.fulfill({
        status: 200,
        json: contactsState.find((c) => c.id === id) ?? mockReglasContact,
      });
    }

    return route.fulfill({
      status: 404,
      json: { message: `E2E mock missing: ${method} ${path}` },
    });
  });
}
