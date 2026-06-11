import { render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import type { NotifCompany } from "../lib/notif.types";
import { CompaniesTable } from "./CompaniesTable";

const company: NotifCompany = {
  id: "11111111-1111-1111-1111-111111111111",
  name: "Flit Renting",
  nit: "900123456",
  contactPhone: "3001234567",
  contactEmail: "ops@flit.dev",
  isActive: true,
  createdAt: "2026-06-01T00:00:00Z",
  updatedAt: null,
};

describe("CompaniesTable", () => {
  it("renderiza filas de compañías", () => {
    render(
      <CompaniesTable items={[company]} onEdit={vi.fn()} onDelete={vi.fn()} />,
    );
    const table = screen.getByTestId("notif-companies-table");
    expect(within(table).getByText("Flit Renting")).toBeTruthy();
    expect(within(table).getByText("ops@flit.dev")).toBeTruthy();
  });
});
