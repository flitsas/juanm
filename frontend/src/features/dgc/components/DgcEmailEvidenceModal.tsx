"use client";

import DOMPurify from "isomorphic-dompurify";
import { FlitModal } from "@/components/flit/modal-form";

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
  const sanitizedHtml = html ? sanitizeEvidenceHtml(html) : null;

  return (
    <FlitModal
      open={open}
      onClose={onClose}
      title="Evidencia del correo"
      titleId="email-evidence-title"
      testId="dgc-email-evidence-modal"
      size="xl"
    >
      {loading ? (
        <p className="text-sm text-[var(--flit-text-secondary)]" role="status">
          Cargando evidencia…
        </p>
      ) : null}

      {error ? (
        <p className="text-sm text-[var(--flit-state-danger)]" role="alert">
          {error}
        </p>
      ) : null}

      {sanitizedHtml && !loading ? (
        <div
          data-testid="email-evidence-html"
          className="prose max-w-none rounded-xl border border-[var(--flit-border-input)] bg-white p-4 text-sm text-[var(--flit-text-primary)]"
          // biome-ignore lint/security/noDangerouslySetInnerHtml: HTML sanitizado con DOMPurify antes de renderizar
          dangerouslySetInnerHTML={{ __html: sanitizedHtml }}
        />
      ) : null}
    </FlitModal>
  );
}
