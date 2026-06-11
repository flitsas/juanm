import { PageHeaderCard } from "@/components/shell/PageHeaderCard";

export default function DashboardPage() {
  return (
    <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="dashboard-page">
      <PageHeaderCard title="Panel de control" subtitle="Visión general y KPIs en tiempo real." />
      <div className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-10 text-center text-sm text-[var(--muted-foreground)]">
        Selecciona <strong className="text-[var(--flit-action)]">Comparendos</strong> en el dock derecho
        para gestionar la maestra de comparendos.
      </div>
    </div>
  );
}
