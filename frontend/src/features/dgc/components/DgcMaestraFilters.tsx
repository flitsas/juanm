import type { ComparendoMaestraFilters } from "../lib/comparendo-maestra.types";

const ESTADOS = ["", "Pendiente", "Impugnado", "Pagado", "Prescrito"] as const;

type DgcMaestraFiltersProps = {
  filters: ComparendoMaestraFilters;
  secretarias: string[];
  onChange: (next: ComparendoMaestraFilters) => void;
};

export function DgcMaestraFilters({ filters, secretarias, onChange }: DgcMaestraFiltersProps) {
  const update = (patch: Partial<ComparendoMaestraFilters>) =>
    onChange({ ...filters, ...patch, page: 1 });

  return (
    <div className="flex flex-wrap items-end gap-3" data-testid="dgc-maestra-filters">
      <label className="min-w-[260px] flex-1">
        <span className="sr-only">Búsqueda global</span>
        <input
          type="search"
          value={filters.search}
          onChange={(e) => update({ search: e.target.value })}
          placeholder="Buscar por comparendo, placa, documento o infractor…"
          className="h-11 w-full rounded-full border border-[var(--border)] bg-[var(--card)] px-4 text-sm outline-none focus:ring-2 focus:ring-[var(--action)]"
          aria-label="Búsqueda global"
        />
      </label>

      <label>
        <span className="mb-1 block text-xs text-[var(--muted-foreground)]">Estado</span>
        <select
          value={filters.estado}
          onChange={(e) => update({ estado: e.target.value })}
          className="h-11 min-w-[9rem] rounded-full border border-[var(--border)] bg-[var(--card)] px-3 text-sm"
          aria-label="Filtrar por estado"
        >
          {ESTADOS.map((estado) => (
            <option key={estado || "todos"} value={estado}>
              {estado || "Todos"}
            </option>
          ))}
        </select>
      </label>

      <label>
        <span className="mb-1 block text-xs text-[var(--muted-foreground)]">Secretaría</span>
        <select
          value={filters.secretaria}
          onChange={(e) => update({ secretaria: e.target.value })}
          className="h-11 min-w-[10rem] rounded-full border border-[var(--border)] bg-[var(--card)] px-3 text-sm"
          aria-label="Filtrar por secretaría"
        >
          <option value="">Todas</option>
          {secretarias.map((s) => (
            <option key={s} value={s}>
              {s.slice(0, 8)}…
            </option>
          ))}
        </select>
      </label>

      <label>
        <span className="mb-1 block text-xs text-[var(--muted-foreground)]">Desde</span>
        <input
          type="date"
          value={filters.fechaDesde}
          onChange={(e) => update({ fechaDesde: e.target.value })}
          className="h-11 rounded-full border border-[var(--border)] bg-[var(--card)] px-3 text-sm"
          aria-label="Fecha desde"
        />
      </label>

      <label>
        <span className="mb-1 block text-xs text-[var(--muted-foreground)]">Hasta</span>
        <input
          type="date"
          value={filters.fechaHasta}
          onChange={(e) => update({ fechaHasta: e.target.value })}
          className="h-11 rounded-full border border-[var(--border)] bg-[var(--card)] px-3 text-sm"
          aria-label="Fecha hasta"
        />
      </label>
    </div>
  );
}
