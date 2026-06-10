"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState, type ReactNode } from "react";
import { isSuperAdmin } from "../lib/roles";
import { getSession } from "../lib/session";
import { LoadingState } from "./ui-state";

type SuperAdminGuardProps = {
  children: ReactNode;
};

export function SuperAdminGuard({ children }: SuperAdminGuardProps) {
  const router = useRouter();
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const session = getSession();
    if (!session) {
      router.replace("/login");
      return;
    }
    if (!isSuperAdmin(session)) {
      router.replace("/dashboard");
      return;
    }
    setReady(true);
  }, [router]);

  if (!ready) {
    return (
      <div className="min-h-screen bg-[var(--flit-bg-app)] p-8">
        <LoadingState label="Verificando permisos de Super Admin…" />
      </div>
    );
  }

  return children;
}
