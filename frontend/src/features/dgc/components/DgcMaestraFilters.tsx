"use client";

import { Search } from "lucide-react";
import { InputText } from "primereact/inputtext";
import { FlitDateField } from "@/components/flit/flit-date-field";
import { FlitSelect } from "@/components/flit/flit-select";
import { FlitFormField } from "@/components/flit/modal-form";
import { COMPARENDO_ESTADO_FILTER_OPTIONS } from "../lib/comparendo-estado-options";
import type { ComparendoMaestraFilters } from "../lib/comparendo-maestra.types";

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

      <div className="min-w-[9rem]">
        <FlitFormField label="Estado" htmlFor="dgc-filter-estado">
          <FlitSelect
            inputId="dgc-filter-estado"
            value={filters.estado}
            options={COMPARENDO_ESTADO_FILTER_OPTIONS}
            onChange={(value) => update({ estado: value })}
            aria-label="Filtrar por estado"
          />
        </FlitFormField>
      </div>

      <div className="min-w-[10rem]">
        <FlitFormField label="Secretaría" htmlFor="dgc-filter-secretaria">
          <FlitSelect
            inputId="dgc-filter-secretaria"
            value={filters.secretaria}
            options={secretariaOptions}
            onChange={(value) => update({ secretaria: value })}
            aria-label="Filtrar por secretaría"
          />
        </FlitFormField>
      </div>

      <div className="min-w-[10rem]">
        <FlitFormField label="Desde" htmlFor="dgc-filter-desde">
          <FlitDateField
            inputId="dgc-filter-desde"
            value={filters.fechaDesde}
            onChange={(value) => update({ fechaDesde: value })}
            aria-label="Fecha desde"
          />
        </FlitFormField>
      </div>

      <div className="min-w-[10rem]">
        <FlitFormField label="Hasta" htmlFor="dgc-filter-hasta">
          <FlitDateField
            inputId="dgc-filter-hasta"
            value={filters.fechaHasta}
            onChange={(value) => update({ fechaHasta: value })}
            aria-label="Fecha hasta"
          />
        </FlitFormField>
      </div>
    </div>
  );
}
