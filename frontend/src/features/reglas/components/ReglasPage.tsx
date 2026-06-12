"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useCallback } from "react";
import { TabPills } from "@/components/flit/tab-pills";
import { PageHeaderCard } from "@/components/shell/PageHeaderCard";
import { RuleBuilderSection } from "./RuleBuilderSection";

type ReglasTab = "constructor" | "ejecucion" | "logs";

const TABS: { id: ReglasTab; label: string }[] = [
  { id: "constructor", label: "Constructor" },
  { id: "ejecucion", label: "Ejecución" },
  { id: "logs", label: "Logs y contactos" },
];

function parseTab(value: string | null): ReglasTab {
  if (value === "ejecucion" || value === "logs" || value === "constructor") {
    return value;
  }
  return "constructor";
}

export function ReglasPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const activeTab = parseTab(searchParams.get("tab"));

  const setTab = useCallback(
    (tab: ReglasTab) => {
      const params = new URLSearchParams(searchParams.toString());
      if (tab === "constructor") {
        params.delete("tab");
      } else {
        params.set("tab", tab);
      }
      const qs = params.toString();
      router.replace(qs ? `/admin/reglas?${qs}` : "/admin/reglas", { scroll: false });
    },
    [router, searchParams],
  );

  return (
    <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="reglas-page">
      <PageHeaderCard
        title="Reglas dinámicas"
        subtitle="Motor REGLAS · Derechos de Petición hacia secretarías de tránsito"
      />

      <TabPills tabs={TABS} active={activeTab} onChange={setTab} ariaLabel="Secciones REGLAS" />

      {activeTab === "constructor" ? <RuleBuilderSection /> : null}

      {activeTab !== "constructor" ? (
        <div
          className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-12 text-center"
          data-testid="reglas-tab-placeholder"
        >
          <p className="text-sm font-medium text-[var(--deep)]">Sección en desarrollo</p>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            La pestaña {activeTab === "ejecucion" ? "Ejecución" : "Logs y contactos"} se entregará en
            las historias #9765 y #9766.
          </p>
        </div>
      ) : null}
    </div>
  );
}
