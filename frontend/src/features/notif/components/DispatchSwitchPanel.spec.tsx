import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { DispatchSwitchPanel } from "./DispatchSwitchPanel";

vi.mock("../api/use-rules", () => ({
  useNotifSwitch: () => ({
    data: { dispatchEnabled: false },
    isLoading: false,
    isError: false,
  }),
  useNotifRuleMutations: () => ({
    setSwitch: { mutateAsync: vi.fn(), isPending: false },
  }),
}));

function renderPanel() {
  const client = new QueryClient();
  return render(
    <QueryClientProvider client={client}>
      <DispatchSwitchPanel />
    </QueryClientProvider>,
  );
}

describe("DispatchSwitchPanel", () => {
  it("muestra estado Off cuando el switch está desactivado", () => {
    renderPanel();
    expect(screen.getByTestId("notif-switch-status").textContent).toContain("Off");
  });
});
