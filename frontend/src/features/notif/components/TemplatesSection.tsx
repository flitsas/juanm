"use client";

import { Button } from "primereact/button";
import { useState } from "react";
import type { NotifTemplate } from "../lib/notif.types";
import { useNotifTemplateMutations, useNotifTemplates } from "../api/use-templates";
import { TemplateEditor } from "./TemplateEditor";

export function TemplatesSection() {
  const templatesQuery = useNotifTemplates();
  const { remove } = useNotifTemplateMutations();
  const [editing, setEditing] = useState<NotifTemplate | null>(null);
  const [creating, setCreating] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const templates = templatesQuery.data?.items ?? [];
  const showEditor = creating || editing !== null;

  const handleDelete = async (template: NotifTemplate) => {
    setDeletingId(template.id);
    try {
      await remove.mutateAsync(template.id);
      if (editing?.id === template.id) {
        setEditing(null);
      }
    } finally {
      setDeletingId(null);
    }
  };

  if (showEditor) {
    return (
      <TemplateEditor
        template={editing}
        onCancel={() => {
          setCreating(false);
          setEditing(null);
        }}
        onSaved={() => {
          setCreating(false);
          setEditing(null);
        }}
      />
    );
  }

  return (
    <div className="space-y-4" data-testid="notif-templates-section">
      <div className="flex justify-end">
        <Button
          type="button"
          label="Nueva plantilla"
          className="flit-btn-primary"
          onClick={() => setCreating(true)}
          data-testid="notif-new-template-btn"
        />
      </div>

      {templatesQuery.isLoading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
        >
          Cargando plantillas…
        </div>
      ) : null}

      {!templatesQuery.isLoading && templatesQuery.isError ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudieron cargar las plantillas.
        </div>
      ) : null}

      {!templatesQuery.isLoading && !templatesQuery.isError && templates.length === 0 ? (
        <div
          className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-12 text-center"
          data-testid="notif-templates-empty"
        >
          <p className="text-sm font-medium text-[var(--deep)]">Sin plantillas configuradas</p>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            Cree una plantilla marca blanca con banner, cuerpo y pie opcionales.
          </p>
        </div>
      ) : null}

      {!templatesQuery.isLoading && !templatesQuery.isError && templates.length > 0 ? (
        <div
          className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
          data-testid="notif-templates-table"
        >
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                <th className="px-4 py-3">Nombre</th>
                <th className="px-4 py-3">Asunto</th>
                <th className="px-4 py-3">Assets</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {templates.map((template) => (
                <tr
                  key={template.id}
                  className="border-t border-[var(--border)]/60 hover:bg-[var(--muted)]/40"
                >
                  <td className="px-4 py-3 font-medium text-[var(--deep)]">{template.name}</td>
                  <td className="px-4 py-3 text-[var(--muted-foreground)]">{template.subject}</td>
                  <td className="px-4 py-3 text-xs text-[var(--muted-foreground)]">
                    {template.bannerUrl ? "Banner" : "—"}
                    {template.bannerUrl && template.footerUrl ? " · " : ""}
                    {template.footerUrl ? "Pie" : ""}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Button
                        type="button"
                        label="Editar"
                        className="flit-btn-secondary p-button-sm"
                        onClick={() => setEditing(template)}
                      />
                      <Button
                        type="button"
                        label="Eliminar"
                        className="flit-btn-danger p-button-sm"
                        loading={deletingId === template.id}
                        onClick={() => void handleDelete(template)}
                      />
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </div>
  );
}
