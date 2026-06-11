"use client";

import { Button } from "primereact/button";
import { useState } from "react";
import { PageHeaderCard } from "@/components/shell/PageHeaderCard";
import { useNotifCompanies, useNotifCompanyMutations } from "../api/use-companies";
import { useNotifProvider } from "../api/use-provider";
import { isSuperAdmin } from "../lib/role-context";
import type { NotifCompany } from "../lib/notif.types";
import { CompaniesTable } from "./CompaniesTable";
import { CompanyFormDialog } from "./CompanyFormDialog";
import { ProviderForm } from "./ProviderForm";
import { TemplatesSection } from "./TemplatesSection";

type AdminTab = "companies" | "provider" | "templates";

export function NotifAdminPage() {
  const superAdmin = isSuperAdmin();
  const [tab, setTab] = useState<AdminTab>(superAdmin ? "companies" : "provider");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<NotifCompany | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const companiesQuery = useNotifCompanies();
  const providerQuery = useNotifProvider();
  const { create, update, remove } = useNotifCompanyMutations();

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
      await create.mutateAsync(payload);
    }
    setDialogOpen(false);
  };

  const companies = companiesQuery.data?.items ?? [];

  return (
    <div className="mx-auto max-w-[1200px] space-y-6 p-6" data-testid="notif-admin-page">
      <PageHeaderCard
        title="Notificaciones"
        subtitle="Motor NOTIF · compañías, proveedor y plantillas email marca blanca"
        actions={
          superAdmin && tab === "companies" ? (
            <Button
              type="button"
              label="Nueva compañía"
              className="flit-btn-primary"
              onClick={openCreate}
              data-testid="notif-new-company-btn"
            />
          ) : null
        }
      />

      <div
        className="flex flex-wrap gap-2"
        role="tablist"
        aria-label="Secciones de notificaciones"
      >
        {superAdmin ? (
          <button
            type="button"
            role="tab"
            aria-selected={tab === "companies"}
            className={`rounded-full px-4 py-2 text-sm font-medium transition ${
              tab === "companies"
                ? "bg-[var(--action)] text-white shadow-md"
                : "bg-[var(--card)] text-[var(--deep)] hover:bg-[var(--muted)]"
            }`}
            onClick={() => setTab("companies")}
            data-testid="notif-tab-companies"
          >
            Compañías
          </button>
        ) : null}
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
      </div>

      {tab === "companies" && superAdmin ? (
        <>
          {companiesQuery.isLoading ? (
            <div
              className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
              role="status"
            >
              Cargando compañías…
            </div>
          ) : null}

          {!companiesQuery.isLoading && companiesQuery.isError ? (
            <div
              className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
              role="alert"
            >
              No se pudo cargar la nómina de compañías.
            </div>
          ) : null}

          {!companiesQuery.isLoading && !companiesQuery.isError && companies.length === 0 ? (
            <div
              className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-12 text-center"
              data-testid="notif-companies-empty"
            >
              <p className="text-sm font-medium text-[var(--deep)]">Sin compañías registradas</p>
              <p className="mt-1 text-sm text-[var(--muted-foreground)]">
                Cree la primera compañía para habilitar tenants en el motor NOTIF.
              </p>
              <Button
                type="button"
                label="Crear compañía"
                className="flit-btn-primary mt-4"
                onClick={openCreate}
              />
            </div>
          ) : null}

          {!companiesQuery.isLoading && !companiesQuery.isError && companies.length > 0 ? (
            <CompaniesTable
              items={companies}
              onEdit={openEdit}
              onDelete={(company) => void handleDelete(company)}
              deletingId={deletingId}
            />
          ) : null}
        </>
      ) : null}

      {tab === "provider" ? (
        <ProviderForm provider={providerQuery.data} isLoading={providerQuery.isLoading} />
      ) : null}

      {tab === "templates" ? <TemplatesSection /> : null}

      <CompanyFormDialog
        open={dialogOpen}
        company={editing}
        saving={create.isPending || update.isPending}
        onOpenChange={setDialogOpen}
        onSubmit={(payload) => void handleSubmit(payload)}
      />
    </div>
  );
}
