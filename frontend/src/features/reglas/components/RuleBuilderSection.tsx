"use client";

import { Button } from "primereact/button";
import { useState } from "react";
import { useReglasContacts, useReglasRuleMutations, useReglasRules } from "../api/use-rules";
import { formatConditionSummary } from "../lib/condition-tree";
import type { ReglasRule, SaveReglasRulePayload } from "../lib/reglas.types";
import { ReglasRuleFormDialog } from "./ReglasRuleFormDialog";

export function RuleBuilderSection() {
  const rulesQuery = useReglasRules();
  const contactsQuery = useReglasContacts();
  const { create, update, remove } = useReglasRuleMutations();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<ReglasRule | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const rules = rulesQuery.data?.items ?? [];
  const contacts = contactsQuery.data?.items ?? [];
  const loading = rulesQuery.isLoading;

  const openCreate = () => {
    setEditing(null);
    setDialogOpen(true);
  };

  const openEdit = (rule: ReglasRule) => {
    setEditing(rule);
    setDialogOpen(true);
  };

  const handleDelete = async (rule: ReglasRule) => {
    setDeletingId(rule.id);
    try {
      await remove.mutateAsync(rule.id);
    } finally {
      setDeletingId(null);
    }
  };

  const handleSubmit = async (payload: SaveReglasRulePayload) => {
    if (editing) {
      await update.mutateAsync({ id: editing.id, payload });
    } else {
      await create.mutateAsync(payload);
    }
    setDialogOpen(false);
  };

  return (
    <div className="space-y-6" data-testid="reglas-rule-builder-section">
      <div className="flex justify-end">
        <Button
          type="button"
          label="Nueva regla DP"
          className="flit-btn-primary"
          onClick={openCreate}
          data-testid="reglas-new-rule-btn"
        />
      </div>

      {loading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
        >
          Cargando reglas dinámicas…
        </div>
      ) : null}

      {!loading && rulesQuery.isError ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudieron cargar las reglas de Derecho de Petición.
        </div>
      ) : null}

      {!loading && !rulesQuery.isError && rules.length === 0 ? (
        <div
          className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-12 text-center"
          data-testid="reglas-rules-empty"
        >
          <p className="text-sm font-medium text-[var(--deep)]">Sin reglas configuradas</p>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            Defina condiciones sobre la maestra DGC, plantilla PDF y correo a secretaría.
          </p>
        </div>
      ) : null}

      {!loading && !rulesQuery.isError && rules.length > 0 ? (
        <div
          className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
          data-testid="reglas-rules-table"
        >
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                <th className="px-4 py-3">Nombre</th>
                <th className="px-4 py-3">Condiciones</th>
                <th className="px-4 py-3">Plantilla PDF</th>
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
                    {formatConditionSummary(rule.conditionRoot)}
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-[var(--muted-foreground)]">
                    {rule.pdfTemplateId.slice(0, 8)}…
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

      <ReglasRuleFormDialog
        open={dialogOpen}
        rule={editing}
        contacts={contacts}
        saving={create.isPending || update.isPending}
        onOpenChange={setDialogOpen}
        onSubmit={(payload) => void handleSubmit(payload)}
      />
    </div>
  );
}
