import { Button } from "primereact/button";
import type { NotifCompany } from "../lib/notif.types";

type CompaniesTableProps = {
  items: NotifCompany[];
  onEdit: (company: NotifCompany) => void;
  onDelete: (company: NotifCompany) => void;
  deletingId?: string | null;
};

export function CompaniesTable({ items, onEdit, onDelete, deletingId }: CompaniesTableProps) {
  return (
    <div
      className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
      data-testid="notif-companies-table"
    >
      <div className="overflow-x-auto">
        <table className="w-full min-w-[720px] border-collapse text-sm">
          <thead>
            <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
              <th className="px-4 py-3">Compañía</th>
              <th className="px-4 py-3">NIT</th>
              <th className="px-4 py-3">Contacto</th>
              <th className="px-4 py-3">Estado</th>
              <th className="px-4 py-3 text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {items.map((company) => (
              <tr
                key={company.id}
                className="border-t border-[var(--border)]/60 hover:bg-[var(--muted)]/40"
              >
                <td className="px-4 py-3 font-medium text-[var(--deep)]">{company.name}</td>
                <td className="px-4 py-3 text-[var(--muted-foreground)]">{company.nit ?? "—"}</td>
                <td className="px-4 py-3 text-[var(--muted-foreground)]">
                  <div>{company.contactEmail ?? "—"}</div>
                  {company.contactPhone ? (
                    <div className="text-xs">{company.contactPhone}</div>
                  ) : null}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${
                      company.isActive
                        ? "bg-[var(--tech)]/20 text-[var(--deep)]"
                        : "bg-[var(--muted)] text-[var(--muted-foreground)]"
                    }`}
                  >
                    {company.isActive ? "Activa" : "Inactiva"}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <div className="flex justify-end gap-2">
                    <Button
                      type="button"
                      label="Editar"
                      className="flit-btn-secondary p-button-sm"
                      onClick={() => onEdit(company)}
                      data-testid={`edit-company-${company.id}`}
                    />
                    <Button
                      type="button"
                      label="Eliminar"
                      className="flit-btn-danger p-button-sm"
                      loading={deletingId === company.id}
                      onClick={() => onDelete(company)}
                      data-testid={`delete-company-${company.id}`}
                    />
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
