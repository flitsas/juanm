"use client";

import { useQueryClient } from "@tanstack/react-query";
import { Button } from "primereact/button";
import { useEffect, useMemo, useState } from "react";
import { PageHeaderCard } from "@/components/shell/PageHeaderCard";
import { dgcQueryKeys } from "../api/query-keys";
import { useComparendosMaestra } from "../api/use-comparendos-maestra";
import type { ComparendoMaestraItem } from "../lib/comparendo-maestra.types";
import { type ComparendoMaestraFilters, DEFAULT_FILTERS } from "../lib/comparendo-maestra.types";
import { MAESTRA_COLUMNS } from "../lib/comparendo-maestra-columns";
import { DgcDetailPanel } from "./DgcDetailPanel";
import { DgcMaestraEmptyState } from "./DgcMaestraEmptyState";
import { DgcMaestraFilters } from "./DgcMaestraFilters";
import { DgcMaestraTable } from "./DgcMaestraTable";
import { DgcOcrUploadDialog } from "./DgcOcrUploadDialog";

export function DgcMaestraPage() {
  const queryClient = useQueryClient();
  const [filters, setFilters] = useState<ComparendoMaestraFilters>(DEFAULT_FILTERS);
  const [debouncedFilters, setDebouncedFilters] =
    useState<ComparendoMaestraFilters>(DEFAULT_FILTERS);
  const [ocrOpen, setOcrOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<ComparendoMaestraItem | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedFilters(filters), 300);
    return () => clearTimeout(timer);
  }, [filters]);

  const { data, isLoading, isError } = useComparendosMaestra(debouncedFilters);

  const secretarias = useMemo(() => {
    const values = new Set<string>();
    for (const item of data?.items ?? []) {
      if (item.secretaria) values.add(item.secretaria);
    }
    return [...values];
  }, [data?.items]);

  const hasActiveFilters =
    filters.search ||
    filters.estado ||
    filters.secretaria ||
    filters.fechaDesde ||
    filters.fechaHasta;

  const invalidateMaestra = () => {
    void queryClient.invalidateQueries({ queryKey: dgcQueryKeys.allComparendos() });
  };

  return (
    <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="dgc-maestra-page">
      <PageHeaderCard
        title="Gestión de Comparendos"
        subtitle={`DGC · trazabilidad total y proyección legal — ${MAESTRA_COLUMNS.length} columnas`}
        actions={
          <Button
            type="button"
            label="Cargar masivo"
            className="flit-btn-primary"
            onClick={() => setOcrOpen(true)}
            data-testid="dgc-cargar-masivo-btn"
          />
        }
      />

      <DgcOcrUploadDialog
        open={ocrOpen}
        onOpenChange={setOcrOpen}
        onConfirmed={invalidateMaestra}
      />

      <DgcDetailPanel
        comparendoId={selectedItem?.id ?? null}
        summaryItem={selectedItem}
        onClose={() => setSelectedItem(null)}
        onUpdated={invalidateMaestra}
      />

      <DgcMaestraFilters filters={filters} secretarias={secretarias} onChange={setFilters} />

      {isLoading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
          aria-live="polite"
        >
          Cargando comparendos…
        </div>
      ) : null}

      {!isLoading && isError ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudo cargar la maestra de comparendos.
        </div>
      ) : null}

      {!isLoading && !isError && data && data.items.length === 0 ? (
        <DgcMaestraEmptyState
          onClearFilters={hasActiveFilters ? () => setFilters(DEFAULT_FILTERS) : undefined}
        />
      ) : null}

      {!isLoading && !isError && data && data.items.length > 0 ? (
        <>
          <DgcMaestraTable
            items={data.items}
            selectedId={selectedItem?.id ?? null}
            onSelectItem={setSelectedItem}
          />
          <footer className="flex flex-wrap items-center justify-between gap-3 text-xs text-[var(--muted-foreground)]">
            <span>
              Mostrando {data.items.length} de {data.totalCount} comparendos
            </span>
            <nav className="flex items-center gap-1" aria-label="Paginación">
              <button
                type="button"
                disabled={data.page <= 1}
                onClick={() => setFilters((f) => ({ ...f, page: f.page - 1 }))}
                className="rounded-full px-3 py-1.5 hover:bg-[var(--muted)] disabled:opacity-40"
                aria-label="Página anterior"
              >
                «
              </button>
              <span className="px-2 font-medium text-[var(--deep)]">
                {data.page} / {Math.max(data.totalPages, 1)}
              </span>
              <button
                type="button"
                disabled={data.page >= data.totalPages}
                onClick={() => setFilters((f) => ({ ...f, page: f.page + 1 }))}
                className="rounded-full px-3 py-1.5 hover:bg-[var(--muted)] disabled:opacity-40"
                aria-label="Página siguiente"
              >
                »
              </button>
            </nav>
          </footer>
        </>
      ) : null}
    </div>
  );
}
