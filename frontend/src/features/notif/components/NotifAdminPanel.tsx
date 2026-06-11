"use client";

import { useState } from "react";
import { useNotifProvider } from "../api/use-provider";
import { isSuperAdmin } from "../lib/role-context";
import { ProviderForm } from "./ProviderForm";
import { RulesSection } from "./RulesSection";
import { TemplatesSection } from "./TemplatesSection";
import { TenantProfilePanel } from "./TenantProfilePanel";

type NotifTab = "provider" | "templates" | "rules";

export function NotifAdminPanel() {
  const [tab, setTab] = useState<NotifTab>("provider");
  const providerQuery = useNotifProvider();

  return (
    <div className="space-y-6" data-testid="notif-admin-page">
      <p className="text-sm text-[var(--muted-foreground)]">
        Motor NOTIF · proveedor, plantillas y reglas de comunicación
      </p>

      <div className="flex flex-wrap gap-2" role="tablist" aria-label="Secciones de notificaciones">
        <button
          type="button"
          role="tab"
          aria-selected={tab === "provider"}
          className={`rounded-full px-4 py-2 text-sm font-medium transition ${
            tab === "provider"
              ? "bg-[var(--action)] text-white shadow-md"
              : "bg-[var(--card)] text-[var(--deep)] hover:bg-[var(--muted)]"
          }`}
          onClick={() => setTab("provider")}
          data-testid="notif-tab-provider"
        >
          Proveedor email
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={tab === "templates"}
          className={`rounded-full px-4 py-2 text-sm font-medium transition ${
            tab === "templates"
              ? "bg-[var(--action)] text-white shadow-md"
              : "bg-[var(--card)] text-[var(--deep)] hover:bg-[var(--muted)]"
          }`}
          onClick={() => setTab("templates")}
          data-testid="notif-tab-templates"
        >
          Plantillas
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={tab === "rules"}
          className={`rounded-full px-4 py-2 text-sm font-medium transition ${
            tab === "rules"
              ? "bg-[var(--action)] text-white shadow-md"
              : "bg-[var(--card)] text-[var(--deep)] hover:bg-[var(--muted)]"
          }`}
          onClick={() => setTab("rules")}
          data-testid="notif-tab-rules"
        >
          Reglas
        </button>
      </div>

      {tab === "provider" ? (
        <div className="space-y-6">
          <TenantProfilePanel />
          <ProviderForm provider={providerQuery.data} isLoading={providerQuery.isLoading} />
        </div>
      ) : null}

      {tab === "templates" ? <TemplatesSection /> : null}

      {tab === "rules" ? <RulesSection /> : null}
    </div>
  );
}
