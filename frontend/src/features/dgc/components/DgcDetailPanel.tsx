"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { dgcQueryKeys } from "../api/query-keys";
import { upsertContraventor } from "../api/upsert-contraventor";
import { useComparendoDetail } from "../api/use-comparendo-detail";
import { useEmailEvidence, useEmailLogs } from "../api/use-email-logs";
import type { ComparendoMaestraItem } from "../lib/comparendo-maestra.types";
import {
  DETAIL_PANEL_TABS,
  type ContraventorForm,
  type DetailPanelTab,
} from "../lib/detail-panel.types";
import { formatDpReadonly } from "../lib/format-dp-readonly";
import { formatMaestraCell } from "../lib/format-maestra-cell";
import { DgcEmailEvidenceModal } from "./DgcEmailEvidenceModal";

type DgcDetailPanelProps = {
  comparendoId: string | null;
  summaryItem: ComparendoMaestraItem | null;
  onClose: () => void;
  onUpdated?: () => void;
};

export function DgcDetailPanel({
  comparendoId,
  summaryItem,
  onClose,
  onUpdated,
}: DgcDetailPanelProps) {
  const queryClient = useQueryClient();
  const [tab, setTab] = useState<DetailPanelTab>("detalle");

  const detailQuery = useComparendoDetail(comparendoId);
  const emailsQuery = useEmailLogs(comparendoId);

  const [contraventorForm, setContraventorForm] = useState<ContraventorForm>({
    nombre: "",
    documento: "",
    correo: "",
  });
  const [savingContraventor, setSavingContraventor] = useState(false);
  const [contraventorMessage, setContraventorMessage] = useState<string | null>(null);

  const [evidenceOpen, setEvidenceOpen] = useState(false);
  const [selectedEmailId, setSelectedEmailId] = useState<string | null>(null);
  const evidenceQuery = useEmailEvidence(selectedEmailId, evidenceOpen);

  useEffect(() => {
    if (!comparendoId) return;

    setTab("detalle");
    setContraventorMessage(null);
    setEvidenceOpen(false);
    setSelectedEmailId(null);
  }, [comparendoId]);

  useEffect(() => {
    const detail = detailQuery.data;
    if (!detail) return;

    setContraventorForm({
      nombre: detail.contraventorNombre ?? "",
      documento: detail.contraventorDocumento ?? "",
      correo: detail.contraventorCorreo ?? "",
    });
  }, [detailQuery.data]);

  const openEvidence = (emailId: string) => {
    setSelectedEmailId(emailId);
    setEvidenceOpen(true);
  };

  const saveContraventor = async () => {
    if (!comparendoId) return;
    if (!contraventorForm.nombre.trim() || !contraventorForm.documento.trim()) {
      setContraventorMessage("Nombre y documento son obligatorios.");
      return;
    }

    setSavingContraventor(true);
    setContraventorMessage(null);
    try {
      await upsertContraventor(comparendoId, contraventorForm);
      await queryClient.invalidateQueries({ queryKey: dgcQueryKeys.allComparendos() });
      setContraventorMessage("Contraventor actualizado correctamente.");
      onUpdated?.();
    } catch {
      setContraventorMessage("Error al guardar el contraventor.");
    } finally {
      setSavingContraventor(false);
    }
  };

  if (!comparendoId) return null;

  const loading = detailQuery.isLoading || emailsQuery.isLoading;
  const error =
    detailQuery.isError || emailsQuery.isError
      ? "No se pudo cargar el detalle del comparendo."
      : null;
  const item = detailQuery.data ?? summaryItem;
  const emails = emailsQuery.data?.items ?? [];
  const dp = formatDpReadonly(item?.dp);

  return (
    <>
      <div
        className="fixed inset-0 z-40 bg-black/30"
        aria-hidden="true"
        onClick={onClose}
        data-testid="dgc-detail-backdrop"
      />
      <aside
        className="fixed inset-y-0 right-0 z-50 flex w-full max-w-lg flex-col border-l border-[var(--border)] bg-[var(--card)] shadow-2xl"
        role="complementary"
        aria-label="Panel de detalle del comparendo"
        data-testid="dgc-detail-panel"
      >
        <header className="border-b border-[var(--border)] px-5 py-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="text-xs text-[var(--muted-foreground)]">Comparendo</p>
              <h2 className="text-xl font-bold text-[var(--deep)]">
                {item?.numeroComparendo ?? comparendoId}
              </h2>
            </div>
            <button
              type="button"
              onClick={onClose}
              className="rounded-full px-3 py-1 text-sm text-[var(--muted-foreground)] hover:bg-[var(--muted)]"
              aria-label="Cerrar panel de detalle"
            >
              Cerrar
            </button>
          </div>

          <nav
            className="mt-4 flex gap-1 rounded-full bg-[var(--muted)]/60 p-1"
            aria-label="Pestañas del detalle"
            data-testid="dgc-detail-tabs"
          >
            {DETAIL_PANEL_TABS.map((t) => (
              <button
                key={t.id}
                type="button"
                role="tab"
                aria-selected={tab === t.id}
                onClick={() => setTab(t.id)}
                className={`flex-1 rounded-full px-3 py-2 text-xs font-medium transition ${
                  tab === t.id
                    ? "bg-[var(--card)] text-[var(--deep)] shadow-sm"
                    : "text-[var(--muted-foreground)]"
                }`}
              >
                {t.label}
              </button>
            ))}
          </nav>
        </header>

        <div className="flex-1 overflow-y-auto p-5">
          {loading ? (
            <p className="text-sm text-[var(--muted-foreground)]" role="status">
              Cargando detalle…
            </p>
          ) : null}

          {error ? (
            <p className="mb-4 text-sm text-[var(--alert)]" role="alert">
              {error}
            </p>
          ) : null}

          {tab === "detalle" && item ? (
            <div className="space-y-3 text-sm" data-testid="dgc-detail-tab-detalle">
              <DetailRow label="Estado" value={item.estado} />
              <DetailRow label="Infractor" value={item.infractor ?? "—"} />
              <DetailRow label="Documento" value={item.documento ?? "—"} />
              <DetailRow label="Placa" value={item.placa ?? "—"} />
              <DetailRow label="Infracción" value={item.infraccion ?? "—"} />
              <DetailRow
                label="Fecha comparendo"
                value={formatMaestraCell(item, "fechaComparendo")}
              />
              <DetailRow
                label="Fecha notificación"
                value={formatMaestraCell(item, "fechaNotificacion")}
              />
              <DetailRow
                label="Días restantes"
                value={formatMaestraCell(item, "diasRestantes")}
              />
              <DetailRow label="Total" value={formatMaestraCell(item, "total")} />
              <DetailRow label="Pago" value={item.pago ?? "—"} />
              <DetailRow label="Fuente" value={item.fuente} />

              <section
                className="mt-4 rounded-2xl border border-[var(--border)] bg-[var(--muted)]/30 p-4"
                data-testid="dgc-dp-readonly"
              >
                <h3 className="text-xs font-semibold uppercase tracking-wide text-[var(--muted-foreground)]">
                  Trámite DP (solo lectura)
                </h3>
                <p className="mt-2 text-xs text-[var(--muted-foreground)]">
                  Consulta de estado — generación de documentos en Feature #9564.
                </p>
                {dp.isLink && dp.href ? (
                  <a
                    href={dp.href}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="mt-2 inline-block text-sm font-medium text-[var(--action)] underline"
                  >
                    {dp.label}
                  </a>
                ) : (
                  <p className="mt-2 text-sm font-medium text-[var(--deep)]">{dp.label}</p>
                )}
              </section>
            </div>
          ) : null}

          {tab === "contraventor" && item ? (
            <div className="space-y-4 text-sm" data-testid="dgc-detail-tab-contraventor">
              <p className="rounded-xl bg-[var(--muted)]/50 px-3 py-2 text-xs text-[var(--muted-foreground)]">
                Asociación actual:{" "}
                <span className="font-medium text-[var(--deep)]">
                  {item.contraventor ?? "Sin asignar"}
                </span>
              </p>

              <label className="block">
                <span className="mb-1 block text-xs text-[var(--muted-foreground)]">Nombre *</span>
                <input
                  value={contraventorForm.nombre}
                  onChange={(e) =>
                    setContraventorForm((f) => ({ ...f, nombre: e.target.value }))
                  }
                  className="h-10 w-full rounded-xl border border-[var(--border)] px-3"
                />
              </label>
              <label className="block">
                <span className="mb-1 block text-xs text-[var(--muted-foreground)]">
                  Documento *
                </span>
                <input
                  value={contraventorForm.documento}
                  onChange={(e) =>
                    setContraventorForm((f) => ({ ...f, documento: e.target.value }))
                  }
                  className="h-10 w-full rounded-xl border border-[var(--border)] px-3"
                />
              </label>
              <label className="block">
                <span className="mb-1 block text-xs text-[var(--muted-foreground)]">Correo</span>
                <input
                  type="email"
                  value={contraventorForm.correo}
                  onChange={(e) =>
                    setContraventorForm((f) => ({ ...f, correo: e.target.value }))
                  }
                  className="h-10 w-full rounded-xl border border-[var(--border)] px-3"
                />
              </label>

              {contraventorMessage ? (
                <p className="text-xs text-[var(--muted-foreground)]" role="status">
                  {contraventorMessage}
                </p>
              ) : null}

              <button
                type="button"
                disabled={savingContraventor}
                onClick={() => void saveContraventor()}
                className="rounded-full bg-[var(--action)] px-5 py-2 text-sm font-medium text-white disabled:opacity-50"
              >
                {savingContraventor ? "Guardando…" : "Guardar contraventor"}
              </button>
            </div>
          ) : null}

          {tab === "correos" ? (
            <div data-testid="dgc-detail-tab-correos">
              {emails.length === 0 ? (
                <p className="text-sm text-[var(--muted-foreground)]">
                  No hay registros de correo para este comparendo.
                </p>
              ) : (
                <ul className="space-y-3">
                  {emails.map((email) => (
                    <li
                      key={email.id}
                      className="rounded-2xl border border-[var(--border)] p-4 text-sm"
                    >
                      <p className="font-medium text-[var(--deep)]">{email.tipoAlerta}</p>
                      <p className="mt-1 text-xs text-[var(--muted-foreground)]">
                        {new Date(email.sentAt).toLocaleString("es-CO")} · {email.estadoEntrega}
                      </p>
                      <p className="mt-1 text-xs">
                        {email.origen} → {email.destino}
                      </p>
                      <button
                        type="button"
                        onClick={() => openEvidence(email.id)}
                        className="mt-3 text-xs font-medium text-[var(--action)] underline"
                        aria-label={`Ver evidencia del correo ${email.tipoAlerta}`}
                      >
                        Ver evidencia HTML
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          ) : null}
        </div>
      </aside>

      <DgcEmailEvidenceModal
        open={evidenceOpen}
        html={evidenceQuery.data?.htmlEvidencia ?? null}
        loading={evidenceQuery.isLoading}
        error={
          evidenceQuery.isError ? "No se pudo cargar la evidencia HTML." : null
        }
        onClose={() => {
          setEvidenceOpen(false);
          setSelectedEmailId(null);
        }}
      />
    </>
  );
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-4 border-b border-[var(--border)]/50 pb-2">
      <span className="text-[var(--muted-foreground)]">{label}</span>
      <span className="text-right font-medium text-[var(--deep)]">{value}</span>
    </div>
  );
}
