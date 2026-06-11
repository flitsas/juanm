"use client";

import { InputSwitch } from "primereact/inputswitch";
import { useState } from "react";
import { useNotifRuleMutations, useNotifSwitch } from "../api/use-rules";

export function DispatchSwitchPanel() {
  const switchQuery = useNotifSwitch();
  const { setSwitch } = useNotifRuleMutations();
  const [notice, setNotice] = useState<string | null>(null);

  const enabled = switchQuery.data?.dispatchEnabled ?? true;
  const loading = switchQuery.isLoading || setSwitch.isPending;

  const handleToggle = async (next: boolean) => {
    setNotice(null);
    try {
      await setSwitch.mutateAsync(next);
      if (!next) {
        setNotice(
          "Switch desactivado. No se encolarán nuevos envíos; la cola pendiente seguirá despachándose.",
        );
      } else {
        setNotice("Switch activado. El motor encolará y despachará según las reglas configuradas.");
      }
    } catch (error) {
      setNotice(error instanceof Error ? error.message : "No se pudo actualizar el switch.");
    }
  };

  if (switchQuery.isLoading) {
    return (
      <div
        className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 text-sm text-[var(--muted-foreground)]"
        role="status"
      >
        Cargando switch operativo…
      </div>
    );
  }

  if (switchQuery.isError) {
    return (
      <div
        className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
        role="alert"
      >
        No se pudo cargar el switch operativo. Configure primero un proveedor de email activo.
      </div>
    );
  }

  return (
    <div
      className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 shadow-sm"
      data-testid="notif-dispatch-switch"
    >
      <div>
        <h2 className="text-lg font-semibold text-[var(--deep)]">Switch operativo</h2>
        <p className="mt-1 text-sm text-[var(--muted-foreground)]">
          Controla el encolado automático de nuevos correos. Con switch Off, los pendientes en cola
          siguen despachándose.
        </p>
        {notice ? (
          <p className="mt-3 text-sm text-[var(--deep)]" role="status">
            {notice}
          </p>
        ) : null}
      </div>
      <div className="flex items-center gap-3">
        <span
          className={`text-sm font-medium ${enabled ? "text-[var(--tech)]" : "text-[var(--alert)]"}`}
          data-testid="notif-switch-status"
        >
          {enabled ? "On — Activo" : "Off — Pausado"}
        </span>
        <InputSwitch
          checked={enabled}
          disabled={loading}
          onChange={(e) => void handleToggle(Boolean(e.value))}
          aria-label="Switch operativo de notificaciones"
          data-testid="notif-switch-toggle"
        />
      </div>
    </div>
  );
}
