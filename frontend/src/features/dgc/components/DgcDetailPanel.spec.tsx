import { fireEvent, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { renderWithQueryClient } from "@/test/render-with-query-client";
import type { ComparendoMaestraItem } from "../lib/comparendo-maestra.types";
import { DgcDetailPanel } from "./DgcDetailPanel";
vi.mock("../api/fetch-comparendo-detail", () => ({
  fetchComparendoDetail: vi.fn().mockResolvedValue({
    id: "cmp-1",
    estado: "Pendiente",
    numeroComparendo: "CMP-500",
    infractor: "Ana",
    documento: "5566",
    placa: "ABC111",
    infraccion: "C01",
    fechaComparendo: "2026-06-01",
    fechaNotificacion: "2026-06-05",
    diasRestantes: 12,
    secretaria: "sec-1",
    total: 300000,
    pago: "No",
    contraventor: "Ana (5566)",
    contraventorNombre: "Ana",
    contraventorDocumento: "5566",
    contraventorCorreo: null,
    dp: "DP-PENDIENTE",
    fuente: "ocr",
  } satisfies ComparendoMaestraItem),
}));

vi.mock("../api/fetch-email-logs", () => ({
  fetchEmailLogs: vi.fn().mockResolvedValue({
    items: [
      {
        id: "email-1",
        sentAt: "2026-06-05T10:00:00Z",
        origen: "notif@flit.co",
        destino: "ana@mail.com",
        cc: null,
        tipoAlerta: "Notificación inicial",
        estadoEntrega: "Entregado",
      },
    ],
  }),
}));

vi.mock("../api/fetch-email-evidence", () => ({
  fetchEmailEvidence: vi.fn().mockResolvedValue({
    id: "email-1",
    htmlEvidencia: "<p>Mensaje de prueba</p>",
  }),
}));

const summary: ComparendoMaestraItem = {
  id: "cmp-1",
  estado: "Pendiente",
  numeroComparendo: "CMP-500",
  infractor: "Ana",
  documento: "5566",
  placa: "ABC111",
  infraccion: "C01",
  fechaComparendo: "2026-06-01",
  fechaNotificacion: "2026-06-05",
  diasRestantes: 12,
  secretaria: "sec-1",
  total: 300000,
  pago: "No",
  contraventor: "Ana (5566)",
  contraventorNombre: "Ana",
  contraventorDocumento: "5566",
  contraventorCorreo: null,
  dp: "DP-PENDIENTE",
  fuente: "ocr",
};

describe("DgcDetailPanel", () => {
  it("renderiza las tres pestañas del detalle", async () => {
    renderWithQueryClient(
      <DgcDetailPanel
        comparendoId="cmp-1"
        summaryItem={summary}
        onClose={vi.fn()}
      />,
    );

    expect(screen.getByTestId("dgc-detail-tabs")).toBeTruthy();
    expect(screen.getByRole("tab", { name: "Detalle" })).toBeTruthy();
    expect(screen.getByRole("tab", { name: "Contraventor" })).toBeTruthy();
    expect(screen.getByRole("tab", { name: "Log de correos" })).toBeTruthy();
  });

  it("muestra sección DP solo lectura en pestaña Detalle", async () => {
    renderWithQueryClient(
      <DgcDetailPanel
        comparendoId="cmp-1"
        summaryItem={summary}
        onClose={vi.fn()}
      />,
    );

    expect(await screen.findByTestId("dgc-dp-readonly")).toBeTruthy();
    expect(screen.getByText("DP-PENDIENTE")).toBeTruthy();
  });

  it("abre modal con HTML de evidencia desde log de correos", async () => {
    renderWithQueryClient(
      <DgcDetailPanel
        comparendoId="cmp-1"
        summaryItem={summary}
        onClose={vi.fn()}
      />,
    );

    fireEvent.click(screen.getByRole("tab", { name: "Log de correos" }));
    expect(await screen.findByText("Notificación inicial")).toBeTruthy();
    fireEvent.click(screen.getByRole("button", { name: /Ver evidencia del correo/i }));

    expect(await screen.findByTestId("dgc-email-evidence-modal")).toBeTruthy();
    expect(await screen.findByTestId("email-evidence-html")).toBeTruthy();
  });
});
