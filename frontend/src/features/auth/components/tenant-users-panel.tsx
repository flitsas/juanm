"use client";

import { Building2, Pencil, Plus, Users } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { PrimaryButton } from "@/components/flit/primary-button";
import { StatusBadge } from "@/components/flit/status-badge";
import { CompanyFormDialog } from "@/features/notif/components/CompanyFormDialog";
import { useNotifCompanies, useNotifCompanyMutations } from "@/features/notif/api/use-companies";
import type { NotifCompany } from "@/features/notif/lib/notif.types";
import type { UserSummary } from "../types";
import { InviteUserForm } from "./invite-user-form";

const PAGE_SIZE = 8;

type TenantUsersPanelProps = {
  accessToken: string;
  users: UserSummary[];
  tenants: { tenantId: string; label: string }[];
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
  const companiesQuery = useNotifCompanies();
  const { create, update, remove } = useNotifCompanyMutations();
  const [activeTenantId, setActiveTenantId] = useState("");
  const [page, setPage] = useState(1);
  const [showInvite, setShowInvite] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<NotifCompany | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const companies = companiesQuery.data?.items ?? [];

  useEffect(() => {
    if (companies.length === 0) {
      setActiveTenantId("");
      return;
    }
    if (!companies.some((company) => company.id === activeTenantId)) {
      setActiveTenantId(companies[0]?.id ?? "");
    }
  }, [companies, activeTenantId]);

  const tenantLabels = useMemo(() => {
    const map = new Map(tenants.map((tenant) => [tenant.tenantId, tenant.label]));
    for (const company of companies) {
      map.set(company.id, company.name);
    }
    return map;
  }, [companies, tenants]);

  const activeCompany = companies.find((company) => company.id === activeTenantId);
  const filtered = useMemo(
    () => users.filter((user) => user.tenantId === activeTenantId),
    [users, activeTenantId],
  );
  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const slice = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const selectTenant = (tenantId: string) => {
    setActiveTenantId(tenantId);
    setPage(1);
    setShowInvite(false);
  };

  const countUsers = (tenantId: string) =>
    users.filter((user) => user.tenantId === tenantId).length;

  const openCreate = () => {
    setEditing(null);
    setDialogOpen(true);
  };

  const openEdit = (company: NotifCompany) => {
    setEditing(company);
    setDialogOpen(true);
  };

  const handleDelete = async (company: NotifCompany) => {
    setDeletingId(company.id);
    try {
      await remove.mutateAsync(company.id);
      setDialogOpen(false);
    } finally {
      setDeletingId(null);
    }
  };

  const handleSubmit = async (
    payload: Parameters<typeof create.mutateAsync>[0] & { isActive?: boolean },
  ) => {
    if (editing) {
      await update.mutateAsync({ id: editing.id, payload });
    } else {
      const created = await create.mutateAsync(payload);
      setActiveTenantId(created.id);
    }
    setDialogOpen(false);
  };

  return (
    <>
      <div className="grid min-h-0 grid-cols-12 gap-4">
        <div
          className="flit-card col-span-12 flex min-h-0 flex-col lg:col-span-4 xl:col-span-3"
          data-testid="admin-companies-sidebar"
        >
          <div className="flex items-center justify-between p-3">
            <div className="flex items-center gap-2 text-sm font-bold text-[var(--flit-text-primary)]">
              <Building2 className="size-4 text-[var(--flit-action)]" aria-hidden="true" />
              Compañías
            </div>
            <PrimaryButton size="sm" onClick={openCreate} data-testid="notif-new-company-btn">
              <Plus className="size-3.5" aria-hidden="true" />
              Nueva
            </PrimaryButton>
          </div>

          <div
            className="scrollbar-thin flex-1 space-y-1.5 overflow-y-auto px-3 pb-3"
            data-vertical-scroll
          >
            {companiesQuery.isLoading ? (
              <p
                className="px-1 py-4 text-center text-xs text-[var(--flit-text-secondary)]"
                role="status"
              >
                Cargando compañías…
              </p>
            ) : null}

            {!companiesQuery.isLoading && companiesQuery.isError ? (
              <p className="px-1 py-4 text-center text-xs text-[var(--flit-alert)]" role="alert">
                No se pudo cargar la nómina de compañías.
              </p>
            ) : null}

            {!companiesQuery.isLoading && !companiesQuery.isError && companies.length === 0 ? (
              <div
                className="rounded-xl border border-dashed border-[var(--flit-border-input)] p-4 text-center"
                data-testid="notif-companies-empty"
              >
                <p className="text-xs font-medium text-[var(--flit-text-primary)]">
                  Sin compañías registradas
                </p>
                <p className="mt-1 text-[11px] text-[var(--flit-text-secondary)]">
                  Cree la primera compañía para habilitar tenants.
                </p>
              </div>
            ) : null}

            {companies.map((company) => {
              const isActive = company.id === activeTenantId;
              return (
                <div
                  key={company.id}
                  className={`relative w-full rounded-xl border transition-all ${
                    isActive
                      ? "border-[var(--flit-action)] bg-[var(--flit-action)]/5 shadow-sm"
                      : "border-[var(--flit-border-input)] hover:border-[var(--flit-action)]/40 hover:bg-[var(--flit-bg-hover)]"
                  }`}
                  data-testid={`company-card-${company.id}`}
                >
                  <button
                    type="button"
                    onClick={() => selectTenant(company.id)}
                    aria-current={isActive ? "true" : undefined}
                    className="w-full p-2.5 pr-10 text-left"
                  >
                    <div className="text-sm font-medium leading-tight">{company.name}</div>
                    <div className="mt-0.5 font-mono text-[11px] text-[var(--flit-text-secondary)]">
                      {company.id}
                    </div>
                    <div className="mt-1.5 flex items-center justify-between text-[11px] text-[var(--flit-text-secondary)]">
                      <span className="inline-flex items-center gap-1">
                        <Users className="size-3" aria-hidden="true" />
                        {countUsers(company.id)}{" "}
                        {countUsers(company.id) === 1 ? "usuario" : "usuarios"}
                      </span>
                      {!company.isActive ? (
                        <span className="text-[var(--flit-alert)]">Inactiva</span>
                      ) : null}
                    </div>
                  </button>
                  <button
                    type="button"
                    onClick={() => openEdit(company)}
                    className="absolute top-2.5 right-2 inline-flex rounded-md p-1 text-[var(--flit-action)] hover:bg-[var(--flit-action)]/10"
                    aria-label={`Editar ${company.name}`}
                    data-testid={`edit-company-${company.id}`}
                  >
                    <Pencil className="size-3.5" aria-hidden="true" />
                  </button>
                </div>
              );
            })}
          </div>
        </div>

        <div className="flit-card col-span-12 flex min-h-0 flex-col lg:col-span-8 xl:col-span-9">
          <div className="flex flex-wrap items-center justify-between gap-3 p-3">
            <div>
              <p className="text-sm font-bold text-[var(--flit-text-primary)]">
                Usuarios de {activeCompany?.name ?? tenantLabels.get(activeTenantId) ?? "—"}
              </p>
              <p className="text-xs font-light text-[var(--flit-text-secondary)]">
                {filtered.length} {filtered.length === 1 ? "miembro" : "miembros"} asociados
              </p>
            </div>
            <PrimaryButton
              size="sm"
              onClick={() => setShowInvite((value) => !value)}
              aria-expanded={showInvite}
              disabled={!activeTenantId}
            >
              <Plus className="size-3.5" aria-hidden="true" />
              {showInvite ? "Cerrar invitación" : "Invitar usuario"}
            </PrimaryButton>
          </div>

          {!activeTenantId ? (
            <div className="flex flex-1 items-center justify-center px-3 pb-6 text-sm text-[var(--flit-text-secondary)]">
              Seleccione o cree una compañía para gestionar usuarios.
            </div>
          ) : (
            <>
              {showInvite ? (
                <div className="border-t border-[var(--flit-border-input)] p-3">
                  <InviteUserForm
                    accessToken={accessToken}
                    tenants={companies.map((company) => ({
                      tenantId: company.id,
                      label: company.name,
                      users: users.filter((user) => user.tenantId === company.id),
                    }))}
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
                    onClick={() => setPage((current) => Math.max(1, current - 1))}
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
                    onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
                    disabled={page === totalPages}
                    className="rounded-md border border-[var(--flit-border-input)] px-2.5 py-1 hover:bg-[var(--flit-bg-hover)] disabled:opacity-50"
                  >
                    Siguiente
                  </button>
                </div>
              </div>
            </>
          )}
        </div>
      </div>

      <CompanyFormDialog
        open={dialogOpen}
        company={editing}
        saving={create.isPending || update.isPending}
        deleting={deletingId === editing?.id}
        onOpenChange={setDialogOpen}
        onSubmit={(payload) => void handleSubmit(payload)}
        onDelete={editing ? () => void handleDelete(editing) : undefined}
      />
    </>
  );
}
