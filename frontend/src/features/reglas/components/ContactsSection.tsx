"use client";

import { Button } from "primereact/button";
import { useState } from "react";
import { useReglasContactMutations } from "../api/use-contacts";
import { useReglasContacts } from "../api/use-rules";
import { canExecuteReglas } from "../lib/execution-labels";
import type { ReglasSecretariatContact } from "../lib/reglas.types";
import { getUserRole } from "../lib/role-context";
import { ContactFormDialog } from "./ContactFormDialog";

export function ContactsSection() {
  const contactsQuery = useReglasContacts();
  const { create, update, remove } = useReglasContactMutations();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<ReglasSecretariatContact | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const contacts = contactsQuery.data?.items ?? [];
  const canManage = canExecuteReglas(getUserRole());

  const openCreate = () => {
    setEditing(null);
    setDialogOpen(true);
  };

  const openEdit = (contact: ReglasSecretariatContact) => {
    setEditing(contact);
    setDialogOpen(true);
  };

  const handleDelete = async (contact: ReglasSecretariatContact) => {
    setDeletingId(contact.id);
    try {
      await remove.mutateAsync(contact.id);
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="space-y-4" data-testid="reglas-contacts-section">
      <div className="flex justify-end">
        <Button
          type="button"
          label="Nuevo contacto"
          className="flit-btn-primary"
          disabled={!canManage}
          onClick={openCreate}
          data-testid="reglas-new-contact-btn"
        />
      </div>

      {contactsQuery.isLoading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-8 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
        >
          Cargando contactos…
        </div>
      ) : null}

      {!contactsQuery.isLoading && contactsQuery.isError ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudieron cargar los contactos de secretaría.
        </div>
      ) : null}

      {!contactsQuery.isLoading && !contactsQuery.isError && contacts.length === 0 ? (
        <div
          className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-8 text-center"
          data-testid="reglas-contacts-empty"
        >
          <p className="text-sm font-medium text-[var(--deep)]">Sin contactos configurados</p>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            Registre correos de despacho por secretaría de tránsito.
          </p>
        </div>
      ) : null}

      {!contactsQuery.isLoading && !contactsQuery.isError && contacts.length > 0 ? (
        <div
          className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--card)]"
          data-testid="reglas-contacts-table"
        >
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="bg-[var(--table-head)] text-left text-xs font-semibold text-[var(--deep)]">
                <th className="px-4 py-3">Secretaría</th>
                <th className="px-4 py-3">Código</th>
                <th className="px-4 py-3">Contacto</th>
                <th className="px-4 py-3">Correo</th>
                <th className="px-4 py-3">Estado</th>
                <th className="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {contacts.map((contact) => (
                <tr key={contact.id} className="border-t border-[var(--border)]/60">
                  <td className="px-4 py-3 font-medium text-[var(--deep)]">
                    {contact.secretariatName}
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-[var(--muted-foreground)]">
                    {contact.secretariatCode}
                  </td>
                  <td className="px-4 py-3">{contact.contactName}</td>
                  <td className="px-4 py-3 text-[var(--muted-foreground)]">{contact.contactEmail}</td>
                  <td className="px-4 py-3">
                    {contact.isActive ? "Activo" : "Inactivo"}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Button
                        type="button"
                        label="Editar"
                        className="flit-btn-secondary p-button-sm"
                        disabled={!canManage}
                        onClick={() => openEdit(contact)}
                      />
                      <Button
                        type="button"
                        label="Eliminar"
                        className="flit-btn-danger p-button-sm"
                        disabled={!canManage}
                        loading={deletingId === contact.id}
                        onClick={() => void handleDelete(contact)}
                      />
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}

      <ContactFormDialog
        open={dialogOpen}
        contact={editing}
        saving={create.isPending || update.isPending}
        onOpenChange={setDialogOpen}
        onSubmit={async (payload) => {
          if (editing) {
            await update.mutateAsync({ id: editing.id, payload });
          } else {
            await create.mutateAsync(payload);
          }
          setDialogOpen(false);
        }}
      />
    </div>
  );
}
