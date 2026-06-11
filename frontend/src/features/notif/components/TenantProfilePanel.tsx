"use client";

import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { useEffect, useState } from "react";
import { useNotifProfileMutations, useNotifTenantProfile } from "../api/use-profile";

export function TenantProfilePanel() {
  const profileQuery = useNotifTenantProfile();
  const { update } = useNotifProfileMutations();
  const [contactPhone, setContactPhone] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!profileQuery.data) return;
    setContactPhone(profileQuery.data.contactPhone ?? "");
    setContactEmail(profileQuery.data.contactEmail ?? "");
  }, [profileQuery.data]);

  if (profileQuery.isLoading) {
    return (
      <div
        className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 text-sm text-[var(--muted-foreground)]"
        role="status"
      >
        Cargando perfil del tenant…
      </div>
    );
  }

  if (profileQuery.isError || !profileQuery.data) {
    return (
      <div
        className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
        role="alert"
      >
        No se pudo cargar el perfil del tenant.
      </div>
    );
  }

  const handleSave = async () => {
    setMessage(null);
    if (contactEmail && !contactEmail.includes("@")) {
      setMessage("Ingrese un correo de contacto válido.");
      return;
    }

    try {
      await update.mutateAsync({
        contactPhone: contactPhone.trim() || undefined,
        contactEmail: contactEmail.trim() || undefined,
      });
      setMessage("Perfil actualizado correctamente.");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "No se pudo guardar el perfil.");
    }
  };

  return (
    <div
      className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 shadow-sm"
      data-testid="notif-tenant-profile"
    >
      <h2 className="text-lg font-semibold text-[var(--deep)]">Perfil del tenant</h2>
      <p className="mt-1 text-sm text-[var(--muted-foreground)]">
        Compañía: <span className="font-medium text-[var(--deep)]">{profileQuery.data.name}</span>
      </p>

      <div className="mt-4 grid gap-4 sm:grid-cols-2">
        <div className="flex flex-col gap-1">
          <label htmlFor="profile-email" className="text-xs text-[var(--muted-foreground)]">
            Correo de contacto
          </label>
          <InputText
            id="profile-email"
            type="email"
            value={contactEmail}
            onChange={(e) => setContactEmail(e.target.value)}
            className="flit-field-input w-full"
          />
        </div>
        <div className="flex flex-col gap-1">
          <label htmlFor="profile-phone" className="text-xs text-[var(--muted-foreground)]">
            Teléfono
          </label>
          <InputText
            id="profile-phone"
            value={contactPhone}
            onChange={(e) => setContactPhone(e.target.value)}
            className="flit-field-input w-full"
          />
        </div>
      </div>

      <div className="mt-4 flex items-center gap-3">
        <Button
          type="button"
          label="Guardar perfil"
          className="flit-btn-primary"
          loading={update.isPending}
          onClick={() => void handleSave()}
          data-testid="profile-save-btn"
        />
        {message ? (
          <span className="text-sm text-[var(--deep)]" role="status">
            {message}
          </span>
        ) : null}
      </div>
    </div>
  );
}
