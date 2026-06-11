"use client";

import { Search } from "lucide-react";
import { Calendar } from "primereact/calendar";
import { Dropdown } from "primereact/dropdown";
import { InputText } from "primereact/inputtext";
import type { ComparendoMaestraFilters } from "../lib/comparendo-maestra.types";
import { COMPARENDO_ESTADO_FILTER_OPTIONS } from "../lib/comparendo-estado-options";
import { formatFilterDate, parseFilterDate } from "../lib/filter-date";

type DgcMaestraFiltersProps = {
  filters: ComparendoMaestraFilters;
  secretarias: string[];
  onChange: (next: ComparendoMaestraFilters) => void;
};

export function DgcMaestraFilters({ filters, secretarias, onChange }: DgcMaestraFiltersProps) {
  const update = (patch: Partial<ComparendoMaestraFilters>) =>
    onChange({ ...filters, ...patch, page: 1 });

  const secretariaOptions = [
    { label: "Todas", value: "" },
    ...secretarias.map((secretaria) => ({
      label: secretaria,
      value: secretaria,
    })),
  ];

  return (
    <div className="flex flex-wrap items-end gap-3" data-testid="dgc-maestra-filters">
      <div className="relative min-w-[260px] flex-1">
        <Search
          className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-[var(--muted-foreground)]"
          aria-hidden
        />
        <InputText
          type="search"
          value={filters.search}
          onChange={(e) => update({ search: e.target.value })}
          placeholder="Buscar por comparendo, placa, documento o infractor…"
          className="flit-field-input flit-field-input--search w-full"
          aria-label="Búsqueda global"
        />
      </div>

      <div className="flex min-w-[9rem] flex-col gap-1">
        <label htmlFor="dgc-filter-estado" className="text-xs text-[var(--muted-foreground)]">
          Estado
        </label>
        <Dropdown
          inputId="dgc-filter-estado"
          value={filters.estado}
          options={COMPARENDO_ESTADO_FILTER_OPTIONS}
          onChange={(e) => update({ estado: (e.value as string) ?? "" })}
          className="flit-dropdown w-full"
          panelClassName="flit-dropdown-panel"
          aria-label="Filtrar por estado"
        />
      </div>

      <div className="flex min-w-[10rem] flex-col gap-1">
        <label htmlFor="dgc-filter-secretaria" className="text-xs text-[var(--muted-foreground)]">
          Secretaría
        </label>
        <Dropdown
          inputId="dgc-filter-secretaria"
          value={filters.secretaria}
          options={secretariaOptions}
          onChange={(e) => update({ secretaria: (e.value as string) ?? "" })}
          className="flit-dropdown w-full"
          panelClassName="flit-dropdown-panel"
          aria-label="Filtrar por secretaría"
        />
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="dgc-filter-desde" className="text-xs text-[var(--muted-foreground)]">
          Desde
        </label>
        <Calendar
          inputId="dgc-filter-desde"
          value={parseFilterDate(filters.fechaDesde)}
          onChange={(e) => update({ fechaDesde: formatFilterDate(e.value as Date | null) })}
          dateFormat="dd/mm/yy"
          showIcon
          showButtonBar
          appendTo={typeof document !== "undefined" ? document.body : undefined}
          className="flit-calendar"
          inputClassName="flit-field-input flit-field-input--calendar"
          panelClassName="flit-datepicker-panel"
          aria-label="Fecha desde"
        />
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="dgc-filter-hasta" className="text-xs text-[var(--muted-foreground)]">
          Hasta
        </label>
        <Calendar
          inputId="dgc-filter-hasta"
          value={parseFilterDate(filters.fechaHasta)}
          onChange={(e) => update({ fechaHasta: formatFilterDate(e.value as Date | null) })}
          dateFormat="dd/mm/yy"
          showIcon
          showButtonBar
          appendTo={typeof document !== "undefined" ? document.body : undefined}
          className="flit-calendar"
          inputClassName="flit-field-input flit-field-input--calendar"
          panelClassName="flit-datepicker-panel"
          aria-label="Fecha hasta"
        />
      </div>
    </div>
  );
}
