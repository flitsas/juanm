"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useMemo } from "react";
import { TabPills } from "@/components/flit/tab-pills";
import { PageHeaderCard } from "@/components/shell/PageHeaderCard";
import { NotifAdminPanel } from "@/features/notif/components/NotifAdminPanel";
import { isSuperAdmin } from "../lib/roles";
import { getSession } from "../lib/session";
import { AdminUsersSection } from "./admin-users-section";
import { RbacMatrixPanel } from "./rbac-matrix-panel";

type AdminTab = "users" | "rbac" | "notificaciones";

const ALL_TABS: { id: AdminTab; label: string }[] = [
  { id: "users", label: "Usuarios y compañías" },
  { id: "rbac", label: "Matriz RBAC" },
  { id: "notificaciones", label: "Notificaciones" },
];

function parseTab(value: string | null, superAdmin: boolean): AdminTab {
  if (value === "rbac" || value === "notificaciones" || value === "users") {
    if (!superAdmin && value !== "notificaciones") {
      return "notificaciones";
    }
    return value;
  }
  return superAdmin ? "users" : "notificaciones";
}

export function UnifiedAdminPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const session = getSession();
  const superAdmin = isSuperAdmin(session);
  const accessToken = session?.accessToken ?? "";

  const activeTab = parseTab(searchParams.get("tab"), superAdmin);

  const tabs = useMemo(
    () => (superAdmin ? ALL_TABS : ALL_TABS.filter((tab) => tab.id === "notificaciones")),
    [superAdmin],
  );

  const setTab = useCallback(
    (tab: AdminTab) => {
      const params = new URLSearchParams(searchParams.toString());
      if (tab === "users") {
        params.delete("tab");
      } else {
        params.set("tab", tab);
      }
      const qs = params.toString();
      router.replace(qs ? `/admin?${qs}` : "/admin", { scroll: false });
    },
    [router, searchParams],
  );

  return (
    <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="unified-admin-page">
      <PageHeaderCard title="Administración" subtitle="Multi-compañía, usuarios y roles" />

      <TabPills
        tabs={tabs}
        active={activeTab}
        onChange={setTab}
        ariaLabel="Secciones de administración"
      />

      {activeTab === "users" && superAdmin ? <AdminUsersSection accessToken={accessToken} /> : null}

      {activeTab === "rbac" && superAdmin ? <RbacMatrixPanel accessToken={accessToken} /> : null}

      {activeTab === "notificaciones" ? <NotifAdminPanel /> : null}
    </div>
  );
}
