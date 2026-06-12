"use client";

import { Button } from "primereact/button";
import { useState } from "react";
import { downloadDerechoPeticionPdf } from "@/features/plantillas/api/plantillas-api";
import { useDerechosPeticionList } from "@/features/plantillas/api/use-plantillas";
import { DP_ESTADO_BADGE_CLASS, formatDpEstadoLabel } from "@/features/plantillas/lib/dp-estado";
import { downloadPdfBlob } from "@/features/plantillas/lib/pdf-download";

type DgcDpGridProps = {
  comparendoId: string;
};

export function DgcDpGrid({ comparendoId }: DgcDpGridProps) {
  const listQuery = useDerechosPeticionList(comparendoId);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  const items = listQuery.data?.items ?? [];

  const handleDownload = async (id: string) => {
    setDownloadingId(id);
    try {
      const blob = await downloadDerechoPeticionPdf(id);
      downloadPdfBlob(blob, `derecho-peticion-${id}.pdf`);
    } finally {
      setDownloadingId(null);
    }
  };

  if (listQuery.isLoading) {
    return (
      <p className="text-sm text-[var(--muted-foreground)]" role="status">
        Cargando derechos de petición…
      </p>
    );
  }

  if (listQuery.isError) {
    return (
      <p className="text-sm text-[var(--alert)]" role="alert">
        No se pudieron cargar los derechos de petición.
      </p>
    );
  }

  if (items.length === 0) {
    return (
      <div
        className="rounded-xl border border-dashed border-[var(--border)] bg-[var(--muted)]/20 p-4 text-center"
        data-testid="gdc-dp-empty"
      >
        <p className="text-sm font-medium text-[var(--deep)]">Sin derechos de petición</p>
        <p className="mt-1 text-xs text-[var(--muted-foreground)]">
          Genere un documento desde Plantillas (/gdc) para este comparendo.
        </p>
      </div>
    );
  }

  return (
    <div
      className="overflow-hidden rounded-2xl border border-[var(--border)]"
      data-testid="gdc-dp-grid"
    >
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
            <th className="px-3 py-2">Generado</th>
            <th className="px-3 py-2">Estado</th>
            <th className="px-3 py-2">Versión</th>
            <th className="px-3 py-2 text-right">Acción</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id} className="border-t border-[var(--border)]/60">
              <td className="px-3 py-2 text-[var(--muted-foreground)]">
                {new Date(item.generatedAt).toLocaleString("es-CO")}
              </td>
              <td className="px-3 py-2">
                <span
                  className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold ${
                    DP_ESTADO_BADGE_CLASS[item.estado] ??
                    "border border-[var(--border)] text-[var(--muted-foreground)]"
                  }`}
                  data-testid={`gdc-dp-estado-${item.id}`}
                >
                  {formatDpEstadoLabel(item.estado)}
                </span>
              </td>
              <td className="px-3 py-2 text-[var(--deep)]">v{item.templateVersion}</td>
              <td className="px-3 py-2 text-right">
                <Button
                  type="button"
                  label="Descargar"
                  className="flit-btn-secondary p-button-sm"
                  loading={downloadingId === item.id}
                  onClick={() => void handleDownload(item.id)}
                  data-testid={`gdc-dp-download-${item.id}`}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
