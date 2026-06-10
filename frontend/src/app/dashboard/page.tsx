import { AuthGuard } from "@/features/auth/components/auth-guard";
import { LogoutButton } from "@/features/auth/components/logout-button";

export const metadata = {
  title: "Panel · GDC 2.0",
};

export default function DashboardPage() {
  return (
    <AuthGuard>
      <main className="min-h-screen bg-[var(--flit-bg-app)] p-8">
        <div className="mx-auto max-w-3xl rounded-[var(--flit-radius-card)] bg-white p-8 shadow-[var(--flit-shadow-card)]">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-sm font-medium text-[var(--flit-text-brand)]">GDC 2.0</p>
              <h1 className="mt-2 text-2xl font-semibold text-[var(--flit-text-primary)]">
                Panel autenticado
              </h1>
              <p className="mt-2 text-sm text-[var(--flit-text-secondary)]">
                Sesión activa. Usa cerrar sesión para volver al login.
              </p>
            </div>
            <LogoutButton />
          </div>
        </div>
      </main>
    </AuthGuard>
  );
}
