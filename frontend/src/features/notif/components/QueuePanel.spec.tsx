import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as useRules from "../api/use-rules";
import { QueuePanel } from "./QueuePanel";

function renderWithQuery(ui: React.ReactElement) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={client}>{ui}</QueryClientProvider>);
}

describe("QueuePanel", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("muestra estado vacío", () => {
    vi.spyOn(useRules, "useNotifQueue").mockReturnValue({
      data: { items: [] },
      isLoading: false,
      isError: false,
    } as unknown as ReturnType<typeof useRules.useNotifQueue>);

    renderWithQuery(<QueuePanel />);
    expect(screen.getByTestId("notif-queue-empty")).toBeTruthy();
  });

  it("renderiza filas de la cola", () => {
    vi.spyOn(useRules, "useNotifQueue").mockReturnValue({
      data: {
        items: [
          {
            id: "dddddddd-dddd-dddd-dddd-dddddddddddd",
            notificationRuleId: "cccccccc-cccc-cccc-cccc-cccccccccccc",
            emailTemplateId: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
            comparendoId: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
            destino: "conductor@flit.dev",
            status: "pending",
            scheduledAt: "2026-06-10T12:00:00Z",
            processedAt: null,
            errorMessage: null,
          },
        ],
      },
      isLoading: false,
      isError: false,
    } as unknown as ReturnType<typeof useRules.useNotifQueue>);

    renderWithQuery(<QueuePanel />);
    const table = screen.getByTestId("notif-queue-table");
    expect(table).toBeTruthy();
    expect(screen.getByText("conductor@flit.dev")).toBeTruthy();
    expect(screen.getByText("Pendiente")).toBeTruthy();
  });
});
