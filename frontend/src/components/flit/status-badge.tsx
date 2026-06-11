const statusStyles: Record<string, string> = {
  Active: "bg-[var(--flit-action)]/15 text-[var(--flit-action)] border-[var(--flit-action)]/40",
  Pending: "bg-[var(--flit-amber)]/20 text-[var(--flit-amber)] border-[var(--flit-amber)]/40",
  Locked: "bg-[var(--flit-state-danger)]/15 text-[var(--flit-state-danger)] border-[var(--flit-state-danger)]/40",
};

type StatusBadgeProps = {
  status: string;
};

export function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <span
      className={`flit-badge capitalize ${statusStyles[status] ?? "border-[var(--flit-border-input)] text-[var(--flit-text-secondary)]"}`}
    >
      {status}
    </span>
  );
}
