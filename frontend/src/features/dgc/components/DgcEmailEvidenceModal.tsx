"use client";

import DOMPurify from "isomorphic-dompurify";

type DgcEmailEvidenceModalProps = {
  open: boolean;
  html: string | null;
  loading: boolean;
  error: string | null;
  onClose: () => void;
};

function sanitizeEvidenceHtml(html: string): string {
  return DOMPurify.sanitize(html, { USE_PROFILES: { html: true } });
}

export function DgcEmailEvidenceModal({
  open,
  html,
  loading,
  error,
  onClose,
}: DgcEmailEvidenceModalProps) {
  if (!open) return null;

  const sanitizedHtml = html ? sanitizeEvidenceHtml(html) : null;

  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="email-evidence-title"
      data-testid="dgc-email-evidence-modal"
    >
      <div className="flex max-h-[90vh] w-full max-w-3xl flex-col overflow-hidden rounded-2xl bg-[var(--card)] shadow-xl">
        <header className="flex items-center justify-between border-b border-[var(--border)] px-5 py-4">
          <h2 id="email-evidence-title" className="text-lg font-bold text-[var(--deep)]">
            Evidencia del correo
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full px-3 py-1 text-sm text-[var(--muted-foreground)] hover:bg-[var(--muted)]"
            aria-label="Cerrar modal de evidencia"
          >
            Cerrar
          </button>
        </header>

        <div className="flex-1 overflow-y-auto p-5">
          {loading ? (
            <p className="text-sm text-[var(--muted-foreground)]" role="status">
              Cargando evidencia…
            </p>
          ) : null}

          {error ? (
            <p className="text-sm text-[var(--alert)]" role="alert">
              {error}
            </p>
          ) : null}

          {sanitizedHtml && !loading ? (
            <div
              data-testid="email-evidence-html"
              className="prose max-w-none rounded-xl border border-[var(--border)] bg-white p-4 text-sm text-[var(--deep)]"
              // biome-ignore lint/security/noDangerouslySetInnerHtml: HTML sanitizado con DOMPurify antes de renderizar
              dangerouslySetInnerHTML={{ __html: sanitizedHtml }}
            />
          ) : null}
        </div>
      </div>
    </div>
  );
}
