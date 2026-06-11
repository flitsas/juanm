type SectionHeaderProps = {
  title: string;
  subtitle?: string;
};

export function SectionHeader({ title, subtitle }: SectionHeaderProps) {
  return (
    <div>
      <h1 className="text-xl font-bold text-[var(--flit-text-primary)] md:text-2xl">{title}</h1>
      {subtitle ? (
        <p className="mt-0.5 text-sm font-light text-[var(--flit-text-secondary)]">{subtitle}</p>
      ) : null}
    </div>
  );
}
