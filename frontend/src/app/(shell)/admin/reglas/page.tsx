import { Suspense } from "react";
import { LoadingState } from "@/features/auth/components/ui-state";
import { ReglasPage } from "@/features/reglas/components/ReglasPage";

export default function AdminReglasPage() {
  return (
    <Suspense fallback={<LoadingState label="Cargando reglas dinámicas…" />}>
      <ReglasPage />
    </Suspense>
  );
}
