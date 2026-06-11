"use client";

import { useState } from "react";
import { PrimaryButton } from "@/components/flit/primary-button";
import { inviteUser } from "../api/admin-api";
import type { TenantGroup } from "../types";

type InviteUserFormProps = {
  accessToken: string;
  tenants: TenantGroup[];
  defaultTenantId?: string;
  onInvited: () => void;
};

export function InviteUserForm({
  accessToken,
  tenants,
  defaultTenantId,
  onInvited,
}: InviteUserFormProps) {
  const [email, setEmail] = useState("");
  const [tenantId, setTenantId] = useState(
    defaultTenantId ?? tenants[0]?.tenantId ?? "",
  );
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);
    setLoading(true);

    try {
      const result = await inviteUser(accessToken, { email, tenantId });
      setSuccess(`Invitación enviada. Usuario ${result.email} en estado ${result.status}.`);
      setEmail("");
      onInvited();
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudo invitar al usuario.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form
      onSubmit={onSubmit}
      className="rounded-[var(--flit-radius-card)] border border-[var(--flit-border-input)] bg-white p-4"
    >
      <h3 className="text-sm font-bold text-[var(--flit-text-primary)]">Invitar usuario</h3>
      <p className="mt-1 text-xs font-light text-[var(--flit-text-secondary)]">
        El usuario quedará en estado pending hasta activar su cuenta.
      </p>

      {error ? (
        <div className="mt-3">
          <p role="alert" className="text-sm text-[var(--flit-state-danger)]">
            {error}
          </p>
        </div>
      ) : null}
      {success ? (
        <p role="status" className="mt-3 text-sm text-[var(--flit-text-brand)]">
          {success}
        </p>
      ) : null}

      <div className="mt-4 space-y-3">
        <div className="space-y-1.5">
          <label htmlFor="invite-email" className="text-xs font-semibold text-[var(--flit-text-primary)]">
            Correo electrónico
          </label>
          <input
            id="invite-email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="flit-input"
            placeholder="nuevo@empresa.co"
          />
        </div>

        <div className="space-y-1.5">
          <label htmlFor="invite-compania" className="text-xs font-semibold text-[var(--flit-text-primary)]">
            Compañía
          </label>
          <select
            id="invite-compania"
            required
            value={tenantId}
            onChange={(e) => setTenantId(e.target.value)}
            className="flit-input"
          >
            {tenants.map((t) => (
              <option key={t.tenantId} value={t.tenantId}>
                {t.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      <PrimaryButton
        type="submit"
        disabled={loading || tenants.length === 0}
        className="mt-4 w-full sm:w-auto"
      >
        {loading ? "Invitando…" : "Invitar usuario"}
      </PrimaryButton>
    </form>
  );
}
