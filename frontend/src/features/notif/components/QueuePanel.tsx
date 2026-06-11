"use client";

import { useNotifQueue } from "../api/use-rules";

const STATUS_LABELS: Record<string, string> = {
  pending: "Pendiente",
  processing: "Procesando",
  sent: "Enviado",
  failed: "Fallido",
};

export function QueuePanel() {
  const queueQuery = useNotifQueue();
  const items = queueQuery.data?.items ?? [];

  if (queueQuery.isLoading) {
    return (
      <div
        className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 text-sm text-[var(--muted-foreground)]"
        role="status"
      >
        Cargando cola de envíos…
      </div>
    );
  }

  if (queueQuery.isError) {
    return (
      <div
        className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
        role="alert"
      >
        No se pudo cargar la cola de envíos.
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div
        className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-8 text-center"
        data-testid="notif-queue-empty"
      >
        <p className="text-sm text-[var(--muted-foreground)]">La cola de envíos está vacía.</p>
      </div>
    );
  }

  return (
    <div
      className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
      data-testid="notif-queue-table"
    >
      <div className="border-b border-[var(--border)] px-4 py-3">
        <h3 className="text-sm font-semibold text-[var(--deep)]">Cola de envíos</h3>
      </div>
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
            <th className="px-4 py-3">Destino</th>
            <th className="px-4 py-3">Estado</th>
            <th className="px-4 py-3">Programado</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id} className="border-t border-[var(--border)]/60">
              <td className="px-4 py-3">{item.destino}</td>
              <td className="px-4 py-3">{STATUS_LABELS[item.status] ?? item.status}</td>
              <td className="px-4 py-3 text-[var(--muted-foreground)]">
                {new Date(item.scheduledAt).toLocaleString("es-CO")}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
