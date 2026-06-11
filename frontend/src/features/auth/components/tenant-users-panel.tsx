"use client";

import { Building2, Pencil, Plus, Users } from "lucide-react";
import { useMemo, useState } from "react";
import { PrimaryButton } from "@/components/flit/primary-button";
import { StatusBadge } from "@/components/flit/status-badge";
import type { TenantGroup, UserSummary } from "../types";
import { InviteUserForm } from "./invite-user-form";

const PAGE_SIZE = 8;

type TenantUsersPanelProps = {
  accessToken: string;
  users: UserSummary[];
  tenants: TenantGroup[];
  onInvited: () => void;
};

function displayNameFromEmail(email: string): string {
  const local = email.split("@")[0] ?? email;
  return local.replace(/[._-]/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());
}

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
    setShowInvite(false);
  };

  return (
    <div className="grid min-h-0 grid-cols-12 gap-4">
      <div className="flit-card col-span-12 flex min-h-0 flex-col lg:col-span-4 xl:col-span-3">
        <div className="flex items-center justify-between p-3">
          <div className="flex items-center gap-2 text-sm font-bold text-[var(--flit-text-primary)]">
            <Building2 className="size-4 text-[var(--flit-action)]" aria-hidden="true" />
            Compañías
          </div>
          <PrimaryButton size="sm" disabled title="Próximamente">
            <Plus className="size-3.5" aria-hidden="true" />
            Nueva
          </PrimaryButton>
        </div>
        <div
          className="scrollbar-thin flex-1 space-y-1.5 overflow-y-auto px-3 pb-3"
          data-vertical-scroll
        >
          {tenants.map((tenant) => {
            const isActive = tenant.tenantId === activeTenantId;
            return (
              <button
                key={tenant.tenantId}
                type="button"
                onClick={() => selectTenant(tenant.tenantId)}
                aria-current={isActive ? "true" : undefined}
                className={`w-full rounded-xl border p-2.5 text-left transition-all ${
                  isActive
                    ? "border-[var(--flit-action)] bg-[var(--flit-action)]/5 shadow-sm"
                    : "border-[var(--flit-border-input)] hover:border-[var(--flit-action)]/40 hover:bg-[var(--flit-bg-hover)]"
                }`}
              >
                <div className="text-sm font-medium leading-tight">{tenant.label}</div>
                <div className="mt-0.5 font-mono text-[11px] text-[var(--flit-text-secondary)]">
                  {tenant.tenantId}
                </div>
                <div className="mt-1.5 flex items-center justify-between text-[11px] text-[var(--flit-text-secondary)]">
                  <span className="inline-flex items-center gap-1">
                    <Users className="size-3" aria-hidden="true" />
                    {tenant.users.length} {tenant.users.length === 1 ? "usuario" : "usuarios"}
                  </span>
                </div>
              </button>
            );
          })}
        </div>
      </div>

      <div className="flit-card col-span-12 flex min-h-0 flex-col lg:col-span-8 xl:col-span-9">
        <div className="flex flex-wrap items-center justify-between gap-3 p-3">
          <div>
            <p className="text-sm font-bold text-[var(--flit-text-primary)]">
              Usuarios de {activeTenant?.label ?? "—"}
            </p>
            <p className="text-xs font-light text-[var(--flit-text-secondary)]">
              {filtered.length} {filtered.length === 1 ? "miembro" : "miembros"} asociados
            </p>
          </div>
          <PrimaryButton
            size="sm"
            onClick={() => setShowInvite((v) => !v)}
            aria-expanded={showInvite}
          >
            <Plus className="size-3.5" aria-hidden="true" />
            {showInvite ? "Cerrar invitación" : "Invitar usuario"}
          </PrimaryButton>
        </div>

        {showInvite ? (
          <div className="border-t border-[var(--flit-border-input)] p-3">
            <InviteUserForm
              accessToken={accessToken}
              tenants={tenants}
              defaultTenantId={activeTenantId}
              onInvited={() => {
                onInvited();
                setShowInvite(false);
              }}
            />
          </div>
        ) : null}

        <div className="scrollbar-thin flex-1 overflow-auto px-3 pb-3" data-vertical-scroll>
          <table className="flit-table w-full">
            <thead>
              <tr>
                <th className="text-left">Nombre</th>
                <th className="text-left">Email</th>
                <th className="text-left">Estado</th>
                <th className="text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {slice.map((user) => (
                <tr key={user.id}>
                  <td className="font-medium">{displayNameFromEmail(user.email)}</td>
                  <td className="text-xs">{user.email}</td>
                  <td>
                    <StatusBadge status={user.status} />
                  </td>
                  <td className="text-right">
                    <button
                      type="button"
                      disabled
                      title="Próximamente"
                      className="inline-flex items-center gap-1 text-xs text-[var(--flit-action)] opacity-50"
                    >
                      <Pencil className="size-3" aria-hidden="true" />
                      Editar
                    </button>
                  </td>
                </tr>
              ))}
              {slice.length === 0 ? (
                <tr>
                  <td
                    colSpan={4}
                    className="py-8 text-center text-sm text-[var(--flit-text-secondary)]"
                  >
                    Esta compañía aún no tiene usuarios.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>

        <div className="mt-2 flex items-center justify-between border-t border-[var(--flit-border-input)] p-3 text-xs text-[var(--flit-text-secondary)]">
          <span>
            Página {page} de {totalPages}
          </span>
          <div className="flex gap-1">
            <button
              type="button"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="rounded-md border border-[var(--flit-border-input)] px-2.5 py-1 hover:bg-[var(--flit-bg-hover)] disabled:opacity-50"
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
              className="rounded-md border border-[var(--flit-border-input)] px-2.5 py-1 hover:bg-[var(--flit-bg-hover)] disabled:opacity-50"
            >
              Siguiente
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
