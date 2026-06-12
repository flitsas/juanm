"use client";

import { X } from "lucide-react";
import type { ReactNode } from "react";

const SIZE_CLASS = {
  sm: "max-w-md",
  md: "max-w-lg",
  lg: "max-w-2xl",
  xl: "max-w-3xl",
  "2xl": "max-w-[960px]",
} as const;

type FlitModalProps = {
  open: boolean;
  onClose: () => void;
  title: string;
  titleId?: string;
  testId?: string;
  size?: keyof typeof SIZE_CLASS;
  children: ReactNode;
  footer?: ReactNode;
};

export function FlitModal({
  open,
  onClose,
  title,
  titleId,
  testId,
  size = "md",
  children,
  footer,
}: FlitModalProps) {
  if (!open) return null;

  const headingId = titleId ?? "flit-modal-title";

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-[2px]"
      role="dialog"
      aria-modal="true"
      aria-labelledby={headingId}
      data-testid={testId}
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
      onKeyDown={(e) => {
        if (e.key === "Escape") onClose();
      }}
    >
      <div
        className={`flit-modal flit-card flex max-h-[90vh] w-full ${SIZE_CLASS[size]} flex-col overflow-hidden`}
      >
        <header className="flex shrink-0 items-center justify-between gap-4 border-b border-[var(--flit-border-input)] px-6 py-4">
          <h2 id={headingId} className="text-lg font-semibold text-[var(--flit-text-primary)]">
            {title}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="inline-flex size-9 items-center justify-center rounded-full text-[var(--flit-text-secondary)] transition hover:bg-[var(--flit-bg-hover)]"
            aria-label="Cerrar diálogo"
          >
            <X className="size-4" aria-hidden />
          </button>
        </header>

        <div className="flit-modal-body flex-1 overflow-y-auto px-6 py-5">{children}</div>

        {footer ? (
          <footer className="shrink-0 border-t border-[var(--flit-border-input)] px-6 py-4">
            {footer}
          </footer>
        ) : null}
      </div>
    </div>
  );
}

type FlitFormFieldProps = {
  label: string;
  htmlFor?: string;
  error?: string;
  hint?: string;
  children: ReactNode;
};

export function FlitFormField({ label, htmlFor, error, hint, children }: FlitFormFieldProps) {
  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={htmlFor} className="text-sm font-medium text-[var(--flit-text-primary)]">
        {label}
      </label>
      {children}
      {hint && !error ? (
        <span className="text-xs text-[var(--flit-text-secondary)]">{hint}</span>
      ) : null}
      {error ? (
        <span className="text-xs text-[var(--flit-state-danger)]" role="alert">
          {error}
        </span>
      ) : null}
    </div>
  );
}

export function FlitModalActions({ children }: { children: ReactNode }) {
  return <div className="flex flex-wrap items-center justify-end gap-3">{children}</div>;
}

export function FlitModalActionsSplit({ start, end }: { start?: ReactNode; end: ReactNode }) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-3">
      <div>{start ?? null}</div>
      <div className="flex flex-wrap items-center gap-3">{end}</div>
    </div>
  );
}
