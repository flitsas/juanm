"use client";

import { useMemo, useState } from "react";
import type { TenantGroup, UserSummary } from "../types";
import { InviteUserForm } from "./invite-user-form";

const PAGE_SIZE = 8;

type TenantUsersPanelProps = {
  accessToken: string;
  users: UserSummary[];
  tenants: TenantGroup[];
  onInvited: () => void;
};

const statusClass: Record<string, string> = {
  Active: "bg-[var(--flit-action)]/15 text-[var(--flit-action)] border-[var(--flit-action)]/40",
  Pending: "bg-amber-100 text-amber-800 border-amber-300",
  Locked: "bg-red-100 text-[var(--flit-state-danger)] border-red-300",
};

export function TenantUsersPanel({
  accessToken,
  users,
  tenants,
  onInvited,
}: TenantUsersPanelProps) {
  const [activeTenantId, setActiveTenantId] = useState(tenants[0]?.tenantId ?? "");
  const [page, setPage] = useState(1);
  const [showInvite, setShowInvite] = useState(false);

  const activeTenant = tenants.find((t) => t.tenantId === activeTenantId);
  const filtered = useMemo(
    () => users.filter((u) => u.tenantId === activeTenantId),
    [users, activeTenantId],
  );
  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const slice = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const selectTenant = (tenantId: string) => {
    setActiveTenantId(tenantId);
    setPage(1);
  };

  return (
    <div className="grid min-h-0 grid-cols-12 gap-4">
      <div className="flit-card col-span-12 flex min-h-0 flex-col lg:col-span-4 xl:col-span-3">
        <div className="border-b border-[var(--flit-border-input)] p-3">
          <p className="text-sm font-bold text-[var(--flit-text-primary)]">Tenants</p>
        </div>
        <div className="flex-1 space-y-1.5 overflow-y-auto p-3" data-vertical-scroll>
          {tenants.map((tenant) => {
            const isActive = tenant.tenantId === activeTenantId;
            return (
              <button
                key={tenant.tenantId}
                type="button"
                onClick={() => selectTenant(tenant.tenantId)}
                className={`w-full rounded-xl border p-2.5 text-left transition-all ${
                  isActive
                    ? "border-[var(--flit-action)] bg-[var(--flit-action)]/5 shadow-sm"
                    : "border-[var(--flit-border-input)] hover:border-[var(--flit-action)]/40 hover:bg-[#f4f7fc]"
                }`}
              >
                <div className="text-sm font-medium leading-tight">{tenant.label}</div>
                <div className="mt-1 font-mono text-[11px] text-[var(--flit-text-secondary)]">
                  {tenant.tenantId}
                </div>
                <div className="mt-1.5 text-[11px] text-[var(--flit-text-secondary)]">
                  {tenant.users.length} {tenant.users.length === 1 ? "usuario" : "usuarios"}
                </div>
              </button>
            );
          })}
        </div>
      </div>

      <div className="flit-card col-span-12 flex min-h-0 flex-col lg:col-span-8 xl:col-span-9">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-[var(--flit-border-input)] p-3">
          <div>
            <p className="text-sm font-bold text-[var(--flit-text-primary)]">
              Usuarios de {activeTenant?.label ?? "—"}
            </p>
            <p className="text-xs font-light text-[var(--flit-text-secondary)]">
              {filtered.length} {filtered.length === 1 ? "miembro" : "miembros"} asociados
            </p>
          </div>
          <button
            type="button"
            onClick={() => setShowInvite((v) => !v)}
            className="flit-gradient-btn h-8 px-4 text-xs"
          >
            {showInvite ? "Cerrar invitación" : "+ Invitar usuario"}
          </button>
        </div>

        {showInvite ? (
          <div className="border-b border-[var(--flit-border-input)] p-3">
            <InviteUserForm accessToken={accessToken} tenants={tenants} onInvited={onInvited} />
          </div>
        ) : null}

        <div className="flex-1 overflow-auto p-3" data-vertical-scroll>
          <table className="flit-table w-full">
            <thead>
              <tr>
                <th className="text-left">Email</th>
                <th className="text-left">Estado</th>
              </tr>
            </thead>
            <tbody>
              {slice.map((user) => (
                <tr key={user.id}>
                  <td className="text-sm">{user.email}</td>
                  <td>
                    <span
                      className={`inline-flex rounded-full border px-2.5 py-0.5 text-xs capitalize ${
                        statusClass[user.status] ?? "border-[var(--flit-border-input)]"
                      }`}
                    >
                      {user.status}
                    </span>
                  </td>
                </tr>
              ))}
              {slice.length === 0 ? (
                <tr>
                  <td colSpan={2} className="py-8 text-center text-sm text-[var(--flit-text-secondary)]">
                    Este tenant aún no tiene usuarios.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>

        <div className="flex items-center justify-between border-t border-[var(--flit-border-input)] p-3 text-xs text-[var(--flit-text-secondary)]">
          <span>
            Página {page} de {totalPages}
          </span>
          <div className="flex gap-1">
            <button
              type="button"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="rounded-md border border-[var(--flit-border-input)] px-2.5 py-1 hover:bg-[#f4f7fc] disabled:opacity-50"
            >
              Anterior
            </button>
            <span className="rounded-md border border-[var(--flit-action)] bg-[var(--flit-action)] px-2.5 py-1 text-white">
              {page}
            </span>
            <button
              type="button"
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="rounded-md border border-[var(--flit-border-input)] px-2.5 py-1 hover:bg-[#f4f7fc] disabled:opacity-50"
            >
              Siguiente
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
