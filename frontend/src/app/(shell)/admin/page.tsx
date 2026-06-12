import { Suspense } from "react";
import { LoadingState } from "@/features/auth/components/ui-state";
import { UnifiedAdminPage } from "@/features/auth/components/unified-admin-page";

export const metadata = {
  title: "Administración · GDC — Gestión De Comparendos",
  description: "Multi-compañía, usuarios y roles",
};

export default function AdminPage() {
  return (
    <Suspense fallback={<LoadingState label="Cargando administración…" />}>
      <UnifiedAdminPage />
    </Suspense>
  );
}
