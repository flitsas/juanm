"use client";

import { Button } from "primereact/button";
import { useMemo, useState } from "react";
import { useNotifRuleMutations, useNotifRules } from "../api/use-rules";
import { useNotifTemplates } from "../api/use-templates";
import type { NotifRule } from "../lib/notif.types";
import { formatRuleTrigger } from "../lib/rule-validation";
import { DispatchSwitchPanel } from "./DispatchSwitchPanel";
import { RuleFormDialog } from "./RuleFormDialog";

export function RulesSection() {
  const rulesQuery = useNotifRules();
  const templatesQuery = useNotifTemplates();
  const { create, update, remove } = useNotifRuleMutations();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<NotifRule | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const templates = templatesQuery.data?.items ?? [];
  const rules = rulesQuery.data?.items ?? [];

  const templateNameById = useMemo(() => {
    const map = new Map<string, string>();
    for (const template of templates) {
      map.set(template.id, template.name);
    }
    return map;
  }, [templates]);

  const openCreate = () => {
    setEditing(null);
    setDialogOpen(true);
  };

  const openEdit = (rule: NotifRule) => {
    setEditing(rule);
    setDialogOpen(true);
  };

  const handleDelete = async (rule: NotifRule) => {
    setDeletingId(rule.id);
    try {
      await remove.mutateAsync(rule.id);
    } finally {
      setDeletingId(null);
    }
  };

  const handleSubmit = async (payload: Parameters<typeof create.mutateAsync>[0]) => {
    if (editing) {
      await update.mutateAsync({ id: editing.id, payload });
    } else {
      await create.mutateAsync(payload);
    }
    setDialogOpen(false);
  };

  const loading = rulesQuery.isLoading || templatesQuery.isLoading;

  return (
    <div className="space-y-6" data-testid="notif-rules-section">
      <DispatchSwitchPanel />

      <div className="flex justify-end">
        <Button
          type="button"
          label="Nueva regla"
          className="flit-btn-primary"
          onClick={openCreate}
          disabled={templates.length === 0}
          data-testid="notif-new-rule-btn"
        />
      </div>

      {templates.length === 0 && !templatesQuery.isLoading ? (
        <div
          className="rounded-2xl border border-[var(--amber-acc)]/40 bg-[var(--amber-acc)]/10 p-4 text-sm text-[var(--deep)]"
          role="alert"
        >
          Cree al menos una plantilla en la pestaña Plantillas antes de definir reglas.
        </div>
      ) : null}

      {loading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
        >
          Cargando reglas…
        </div>
      ) : null}

      {!loading && rulesQuery.isError ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudieron cargar las reglas de comunicación.
        </div>
      ) : null}

      {!loading && !rulesQuery.isError && rules.length === 0 ? (
        <div
          className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-12 text-center"
          data-testid="notif-rules-empty"
        >
          <p className="text-sm font-medium text-[var(--deep)]">Sin reglas configuradas</p>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            Defina disparadores cronológicos o por estado del comparendo.
          </p>
        </div>
      ) : null}

      {!loading && !rulesQuery.isError && rules.length > 0 ? (
        <div
          className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
          data-testid="notif-rules-table"
        >
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                <th className="px-4 py-3">Nombre</th>
                <th className="px-4 py-3">Plantilla</th>
                <th className="px-4 py-3">Disparador</th>
                <th className="px-4 py-3">Estado</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {rules.map((rule) => (
                <tr
                  key={rule.id}
                  className="border-t border-[var(--border)]/60 hover:bg-[var(--muted)]/40"
                >
                  <td className="px-4 py-3 font-medium text-[var(--deep)]">{rule.name}</td>
                  <td className="px-4 py-3 text-[var(--muted-foreground)]">
                    {templateNameById.get(rule.emailTemplateId) ?? rule.emailTemplateId}
                  </td>
                  <td className="px-4 py-3 text-[var(--muted-foreground)]">
                    {formatRuleTrigger(rule)}
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        rule.isActive
                          ? "bg-[var(--tech)]/20 text-[var(--deep)]"
                          : "bg-[var(--muted)] text-[var(--muted-foreground)]"
                      }`}
                    >
                      {rule.isActive ? "Activa" : "Inactiva"}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Button
                        type="button"
                        label="Editar"
                        className="flit-btn-secondary p-button-sm"
                        onClick={() => openEdit(rule)}
                      />
                      <Button
                        type="button"
                        label="Eliminar"
                        className="flit-btn-danger p-button-sm"
                        loading={deletingId === rule.id}
                        onClick={() => void handleDelete(rule)}
                      />
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}

      <RuleFormDialog
        open={dialogOpen}
        rule={editing}
        templates={templates}
        saving={create.isPending || update.isPending}
        onOpenChange={setDialogOpen}
        onSubmit={(payload) => void handleSubmit(payload)}
      />
    </div>
  );
}
