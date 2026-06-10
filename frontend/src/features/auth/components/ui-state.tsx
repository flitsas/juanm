type LoadingStateProps = {
  label: string;
};

export function LoadingState({ label }: LoadingStateProps) {
  return (
    <div
      role="status"
      aria-live="polite"
      className="flex flex-col items-center justify-center gap-2 py-16 text-sm text-[var(--flit-text-secondary)]"
    >
      <span
        className="inline-block h-8 w-8 animate-spin rounded-full border-2 border-[var(--flit-action)] border-t-transparent"
        aria-hidden="true"
      />
      {label}
    </div>
  );
}

type EmptyStateProps = {
  title: string;
  description: string;
};

export function EmptyState({ title, description }: EmptyStateProps) {
  return (
    <div className="py-12 text-center">
      <p className="text-sm font-semibold text-[var(--flit-text-primary)]">{title}</p>
      <p className="mt-1 text-sm font-light text-[var(--flit-text-secondary)]">{description}</p>
    </div>
  );
}

type ErrorStateProps = {
  message: string;
};

export function ErrorState({ message }: ErrorStateProps) {
  return (
    <p
      role="alert"
      className="rounded-[var(--flit-radius-input)] border border-[var(--flit-state-danger)] bg-[#FFF5F3] px-4 py-3 text-sm text-[var(--flit-state-danger)]"
    >
      {message}
    </p>
  );
}
