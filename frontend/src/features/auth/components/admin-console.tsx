"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import { SectionHeader } from "@/components/flit/section-header";
import { TabPills } from "@/components/flit/tab-pills";
import { listAdminUsers } from "../api/admin-api";
import { groupUsersByTenant } from "../lib/tenants";
import { getSession } from "../lib/session";
import { LogoutButton } from "./logout-button";
import { RbacMatrixPanel } from "./rbac-matrix-panel";
import { TenantUsersPanel } from "./tenant-users-panel";
import { EmptyState, ErrorState, LoadingState } from "./ui-state";

type AdminTab = "users" | "rbac";

const ADMIN_TABS: { id: AdminTab; label: string }[] = [
  { id: "users", label: "Usuarios y compañías" },
  { id: "rbac", label: "Matriz RBAC" },
];

export function AdminConsole() {
  const session = getSession();
  const accessToken = session?.accessToken ?? "";
  const [tab, setTab] = useState<AdminTab>("users");
  const [users, setUsers] = useState<Awaited<ReturnType<typeof listAdminUsers>>>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await listAdminUsers(accessToken);
      setUsers(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudo cargar la consola.");
    } finally {
      setLoading(false);
    }
  }, [accessToken]);

  useEffect(() => {
    if (accessToken) {
      void loadUsers();
    }
  }, [accessToken, loadUsers]);

  const tenants = useMemo(() => groupUsersByTenant(users), [users]);

  return (
    <div className="min-h-screen bg-[var(--flit-bg-app)] p-4 md:p-8">
      <header className="mx-auto mb-6 flex max-w-7xl flex-wrap items-center justify-between gap-4 border-b border-[var(--flit-border-input)] pb-4">
        <SectionHeader
          title="Administración"
          subtitle="Multi-compañía, usuarios y roles"
        />
        <div className="flex items-center gap-3">
          <Link
            href="/dashboard"
            className="text-sm text-[var(--flit-text-brand)] underline-offset-2 hover:underline"
          >
            Panel
          </Link>
          <LogoutButton />
        </div>
      </header>

      <div className="mx-auto max-w-7xl">
        <TabPills
          tabs={ADMIN_TABS}
          active={tab}
          onChange={setTab}
          ariaLabel="Secciones de administración"
        />

        {tab === "users" ? (
          loading ? (
            <LoadingState label="Cargando usuarios…" />
          ) : error ? (
            <ErrorState message={error} />
          ) : users.length === 0 ? (
            <div className="flit-card p-6">
              <EmptyState
                title="Sin usuarios registrados"
                description="Aún no hay usuarios en el sistema. Invita el primero desde una compañía existente."
              />
            </div>
          ) : (
            <TenantUsersPanel
              accessToken={accessToken}
              users={users}
              tenants={tenants}
              onInvited={loadUsers}
            />
          )
        ) : (
          <RbacMatrixPanel accessToken={accessToken} />
        )}
      </div>
    </div>
  );
}
