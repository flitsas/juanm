"use client";

import { useRouter } from "next/navigation";
import { useCallback, useEffect, useMemo, useState } from "react";
import { listAdminUsers } from "../api/admin-api";
import { SessionExpiredError } from "../lib/ensure-access-token";
import { groupUsersByTenant } from "../lib/tenants";
import { TenantUsersPanel } from "./tenant-users-panel";
import { ErrorState, LoadingState } from "./ui-state";

type AdminUsersSectionProps = {
  accessToken: string;
};

export function AdminUsersSection({ accessToken }: AdminUsersSectionProps) {
  const router = useRouter();
  const [users, setUsers] = useState<Awaited<ReturnType<typeof listAdminUsers>>>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await listAdminUsers(accessToken);
      setUsers(data);
    } catch (err) {
      if (err instanceof SessionExpiredError) {
        router.replace("/login");
        return;
      }
      setError(err instanceof Error ? err.message : "No se pudo cargar la consola.");
    } finally {
      setLoading(false);
    }
  }, [accessToken, router]);

  useEffect(() => {
    if (accessToken) {
      void loadUsers();
    }
  }, [accessToken, loadUsers]);

  const tenants = useMemo(() => groupUsersByTenant(users), [users]);

  if (loading) {
    return <LoadingState label="Cargando usuarios…" />;
  }

  if (error) {
    return <ErrorState message={error} />;
  }

  return (
    <TenantUsersPanel
      accessToken={accessToken}
      users={users}
      tenants={tenants}
      onInvited={loadUsers}
    />
  );
}
