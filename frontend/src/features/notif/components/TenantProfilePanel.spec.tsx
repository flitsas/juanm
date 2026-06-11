import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as useProfile from "../api/use-profile";
import { TenantProfilePanel } from "./TenantProfilePanel";

function renderWithQuery(ui: React.ReactElement) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={client}>{ui}</QueryClientProvider>);
}

const profile = {
  tenantId: "22222222-2222-2222-2222-222222222222",
  name: "Flit Renting",
  contactPhone: "3001234567",
  contactEmail: "ops@flit.dev",
};

describe("TenantProfilePanel", () => {
  beforeEach(() => {
    vi.spyOn(useProfile, "useNotifTenantProfile").mockReturnValue({
      data: profile,
      isLoading: false,
      isError: false,
    } as ReturnType<typeof useProfile.useNotifTenantProfile>);

    vi.spyOn(useProfile, "useNotifProfileMutations").mockReturnValue({
      update: {
        mutateAsync: vi.fn().mockResolvedValue(profile),
        isPending: false,
      },
    } as unknown as ReturnType<typeof useProfile.useNotifProfileMutations>);
  });

  it("muestra datos del perfil y permite guardar", async () => {
    const mutations = useProfile.useNotifProfileMutations();
    renderWithQuery(<TenantProfilePanel />);

    expect(screen.getByTestId("notif-tenant-profile")).toBeTruthy();
    expect(screen.getByText("Flit Renting")).toBeTruthy();

    fireEvent.change(screen.getByLabelText("Correo de contacto"), {
      target: { value: "nuevo@flit.dev" },
    });
    fireEvent.click(screen.getByTestId("profile-save-btn"));

    await waitFor(() => {
      expect(mutations.update.mutateAsync).toHaveBeenCalledWith({
        contactPhone: "3001234567",
        contactEmail: "nuevo@flit.dev",
      });
    });
    expect(await screen.findByText("Perfil actualizado correctamente.")).toBeTruthy();
  });

  it("valida correo inválido", async () => {
    renderWithQuery(<TenantProfilePanel />);
    fireEvent.change(screen.getByLabelText("Correo de contacto"), {
      target: { value: "correo-invalido" },
    });
    fireEvent.click(screen.getByTestId("profile-save-btn"));
    expect(await screen.findByText("Ingrese un correo de contacto válido.")).toBeTruthy();
  });
});
