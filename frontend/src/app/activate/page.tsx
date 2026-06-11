import { Suspense } from "react";
import { ActivateAccountForm } from "@/features/auth/components/activate-account-form";
import { AuthShell } from "@/features/auth/components/auth-shell";

export const metadata = {
  title: "Activar cuenta · GDC — Gestión De Comparendos",
  description: "Activa tu cuenta en GDC (Gestión De Comparendos) con el enlace de invitación",
};

function ActivateFallback() {
  return (
    <div>
      <h1 className="text-3xl font-bold text-[var(--flit-text-primary)]">Activar cuenta</h1>
      <p className="mt-4 text-sm text-[var(--flit-text-secondary)]">Cargando…</p>
    </div>
  );
}

export default function ActivatePage() {
  return (
    <AuthShell intro={false}>
      <Suspense fallback={<ActivateFallback />}>
        <ActivateAccountForm />
      </Suspense>
    </AuthShell>
  );
}
