type DgcMaestraEmptyStateProps = {
  onClearFilters?: () => void;
};

export function DgcMaestraEmptyState({ onClearFilters }: DgcMaestraEmptyStateProps) {
  return (
    <div
      className="flex flex-col items-center justify-center gap-4 rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] px-8 py-16 text-center"
      data-testid="dgc-maestra-empty"
    >
      <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-flit text-2xl text-white shadow-md">
        📋
      </div>
      <div>
        <h2 className="text-lg font-bold text-[var(--deep)]">Sin comparendos</h2>
        <p className="mt-2 max-w-md text-sm text-[var(--muted-foreground)]">
          No hay comparendos para este tenant. Carga comparendos vía OCR o registro manual
          para comenzar a gestionar la maestra DGC.
        </p>
      </div>
      {onClearFilters ? (
        <button
          type="button"
          onClick={onClearFilters}
          className="rounded-full bg-[var(--action)] px-5 py-2.5 text-sm font-medium text-white transition hover:opacity-90"
          aria-label="Limpiar filtros de búsqueda"
        >
          Limpiar filtros
        </button>
      ) : null}
    </div>
  );
}
