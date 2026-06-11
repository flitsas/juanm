"use client";

import DOMPurify from "isomorphic-dompurify";

type TemplatePreviewPanelProps = {
  subject: string;
  html: string;
};

function sanitizePreviewHtml(html: string): string {
  return DOMPurify.sanitize(html, { USE_PROFILES: { html: true } });
}

export function TemplatePreviewPanel({ subject, html }: TemplatePreviewPanelProps) {
  const sanitized = sanitizePreviewHtml(html);

  return (
    <div
      className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-4"
      data-testid="notif-template-preview"
    >
      <h3 className="text-sm font-semibold text-[var(--deep)]">Vista previa</h3>
      <p className="mt-2 text-xs text-[var(--muted-foreground)]">
        Asunto: <span className="font-medium text-[var(--deep)]">{subject || "—"}</span>
      </p>
      <div
        className="mt-4 overflow-auto rounded-xl border border-[var(--border)] bg-white p-4 text-sm text-[var(--deep)]"
        // biome-ignore lint/security/noDangerouslySetInnerHtml: HTML sanitizado con DOMPurify
        dangerouslySetInnerHTML={{ __html: sanitized }}
      />
    </div>
  );
}
