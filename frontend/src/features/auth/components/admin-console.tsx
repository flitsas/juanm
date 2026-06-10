"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import { listAdminUsers } from "../api/admin-api";
import { groupUsersByTenant } from "../lib/tenants";
import { getSession } from "../lib/session";
import { LogoutButton } from "./logout-button";
import { RbacMatrixPanel } from "./rbac-matrix-panel";
import { TenantUsersPanel } from "./tenant-users-panel";
import { EmptyState, ErrorState, LoadingState } from "./ui-state";

type AdminTab = "users" | "rbac";

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
      <header className="mx-auto mb-6 flex max-w-7xl flex-wrap items-center justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-[var(--flit-text-brand)]">GDC 2.0 · Super Admin</p>
          <h1 className="mt-1 text-2xl font-bold text-[var(--flit-text-primary)]">
            Consola de administración
          </h1>
        </div>
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
        <nav aria-label="Secciones de administración" className="mb-4 flex gap-2">
          <button
            type="button"
            onClick={() => setTab("users")}
            className={`rounded-full px-4 py-2 text-sm font-medium ${
              tab === "users"
                ? "bg-[var(--flit-action)] text-white"
                : "border border-[var(--flit-border-input)] bg-white text-[var(--flit-text-primary)]"
            }`}
          >
            Usuarios
          </button>
          <button
            type="button"
            onClick={() => setTab("rbac")}
            className={`rounded-full px-4 py-2 text-sm font-medium ${
              tab === "rbac"
                ? "bg-[var(--flit-action)] text-white"
                : "border border-[var(--flit-border-input)] bg-white text-[var(--flit-text-primary)]"
            }`}
          >
            Matriz RBAC
          </button>
        </nav>

        {tab === "users" ? (
          loading ? (
            <LoadingState label="Cargando usuarios…" />
          ) : error ? (
            <ErrorState message={error} />
          ) : users.length === 0 ? (
            <div className="flit-card p-6">
              <EmptyState
                title="Sin usuarios registrados"
                description="Aún no hay usuarios en el sistema. Invita el primero desde un tenant existente."
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
