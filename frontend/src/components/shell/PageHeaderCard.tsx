type PageHeaderCardProps = {
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
};

/** Título en tarjeta blanca — patrón Section de flitready-suite */
export function PageHeaderCard({ title, subtitle, actions }: PageHeaderCardProps) {
  return (
    <div className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-gradient-flit text-2xl font-bold md:text-3xl">{title}</h1>
          {subtitle ? (
            <p className="mt-1 text-sm text-[var(--muted-foreground)]">{subtitle}</p>
          ) : null}
        </div>
        {actions ? <div className="flex items-center gap-3">{actions}</div> : null}
      </div>
    </div>
  );
}
