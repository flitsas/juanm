import { MAESTRA_COLUMNS } from "../lib/comparendo-maestra-columns";
import type { ComparendoMaestraItem } from "../lib/comparendo-maestra.types";
import { formatMaestraCell } from "../lib/format-maestra-cell";

const estadoClass: Record<string, string> = {
  Pendiente: "bg-[var(--amber-acc)]/20 text-[var(--deep)]",
  Impugnado: "bg-[var(--action)]/15 text-[var(--action)]",
  Pagado: "bg-[var(--tech)]/20 text-[var(--deep)]",
  Prescrito: "bg-[var(--alert)]/15 text-[var(--alert)]",
};

type DgcMaestraTableProps = {
  items: ComparendoMaestraItem[];
  onSelectItem?: (item: ComparendoMaestraItem) => void;
  selectedId?: string | null;
};

export function DgcMaestraTable({ items, onSelectItem, selectedId }: DgcMaestraTableProps) {
  return (
    <div
      className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
      data-testid="dgc-maestra-table"
    >
      <div className="overflow-x-auto">
        <table className="w-full min-w-[1200px] border-collapse text-sm">
          <thead>
            <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
              {MAESTRA_COLUMNS.map((col) => (
                <th key={col.key} className="whitespace-nowrap px-3 py-3">
                  {col.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr
                key={item.id}
                className={`cursor-pointer border-t border-[var(--border)]/60 transition hover:bg-[var(--muted)]/60 ${
                  selectedId === item.id ? "bg-[var(--action)]/10" : ""
                }`}
                onClick={() => onSelectItem?.(item)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    onSelectItem?.(item);
                  }
                }}
                tabIndex={onSelectItem ? 0 : undefined}
                role={onSelectItem ? "button" : undefined}
                aria-label={onSelectItem ? `Abrir detalle ${item.numeroComparendo}` : undefined}
              >
                {MAESTRA_COLUMNS.map((col) => (
                  <td key={col.key} className="whitespace-nowrap px-3 py-2.5">
                    {col.key === "estado" ? (
                      <span
                        className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${estadoClass[item.estado] ?? "bg-[var(--muted)]"}`}
                      >
                        {item.estado}
                      </span>
                    ) : (
                      formatMaestraCell(item, col.key)
                    )}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
