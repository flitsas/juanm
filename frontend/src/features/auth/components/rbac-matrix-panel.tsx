"use client";

import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { PrimaryButton } from "@/components/flit/primary-button";
import { getRbacMatrix, updateRbacMatrix } from "../api/admin-api";
import { SessionExpiredError } from "../lib/ensure-access-token";
import type { RbacMatrix } from "../types";
import { EmptyState, ErrorState, LoadingState } from "./ui-state";

type RbacMatrixPanelProps = {
  accessToken: string;
};

function buildAssignmentMap(matrix: RbacMatrix): Map<string, boolean> {
  const map = new Map<string, boolean>();
  for (const role of matrix.roles) {
    for (const permission of matrix.permissions) {
      const key = `${role.roleId}:${permission.id}`;
      map.set(key, role.permissionIds.includes(permission.id));
    }
  }
  return map;
}

export function RbacMatrixPanel({ accessToken }: RbacMatrixPanelProps) {
  const router = useRouter();
  const [matrix, setMatrix] = useState<RbacMatrix | null>(null);
  const [assignments, setAssignments] = useState<Map<string, boolean>>(new Map());
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getRbacMatrix(accessToken);
      setMatrix(data);
      setAssignments(buildAssignmentMap(data));
    } catch (err) {
      if (err instanceof SessionExpiredError) {
        router.replace("/login");
        return;
      }
      setError(err instanceof Error ? err.message : "No se pudo cargar la matriz RBAC.");
    } finally {
      setLoading(false);
    }
  }, [accessToken, router]);

  useEffect(() => {
    void load();
  }, [load]);

  const toggle = (roleId: string, permissionId: string) => {
    const key = `${roleId}:${permissionId}`;
    setAssignments((prev) => {
      const next = new Map(prev);
      next.set(key, !prev.get(key));
      return next;
    });
    setSuccess(null);
  };

  const onSave = async () => {
    if (!matrix) return;
    setSaving(true);
    setError(null);
    setSuccess(null);

    const updates = matrix.roles.flatMap((role) =>
      matrix.permissions.map((permission) => ({
        roleId: role.roleId,
        permissionId: permission.id,
        enabled: assignments.get(`${role.roleId}:${permission.id}`) ?? false,
      })),
    );

    try {
      await updateRbacMatrix(accessToken, updates);
      await load();
      setSuccess("Matriz RBAC actualizada correctamente.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudo guardar la matriz.");
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <LoadingState label="Cargando matriz de permisos…" />;
  }

  if (error && !matrix) {
    return <ErrorState message={error} />;
  }

  if (!matrix || matrix.permissions.length === 0) {
    return (
      <div className="flit-card p-6">
        <EmptyState
          title="Sin permisos configurados"
          description="No hay permisos disponibles en la matriz RBAC."
        />
      </div>
    );
  }

  return (
    <div className="flit-card flex min-h-0 flex-col">
      <div className="flex flex-wrap items-center justify-between gap-3 p-3">
        <div>
          <h2 className="text-sm font-bold text-[var(--flit-text-primary)]">Matriz RBAC</h2>
          <p className="text-xs font-light text-[var(--flit-text-secondary)]">
            Asigna permisos funcionales por rol
          </p>
        </div>
        <PrimaryButton type="button" onClick={onSave} disabled={saving} size="sm">
          {saving ? "Guardando…" : "Guardar cambios"}
        </PrimaryButton>
      </div>

      {error ? (
        <div className="px-3 pb-3">
          <ErrorState message={error} />
        </div>
      ) : null}
      {success ? (
        <p role="status" className="px-3 pb-3 text-sm text-[var(--flit-text-brand)]">
          {success}
        </p>
      ) : null}

      <div className="scrollbar-thin overflow-auto px-3 pb-3" data-vertical-scroll>
        <table className="flit-table w-full min-w-[640px]">
          <thead>
            <tr>
              <th className="text-left">Permiso</th>
              {matrix.roles.map((role) => (
                <th key={role.roleId} className="text-center">
                  {role.name}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {matrix.permissions.map((permission) => (
              <tr key={permission.id}>
                <td className="font-medium">
                  <span className="text-sm">{permission.code}</span>
                  <span className="mt-0.5 block text-[11px] font-light text-[var(--flit-text-secondary)]">
                    {permission.module} · {permission.action}
                  </span>
                </td>
                {matrix.roles.map((role) => {
                  const checked = assignments.get(`${role.roleId}:${permission.id}`) ?? false;
                  const label = `${permission.code} ${role.name}`;
                  return (
                    <td key={role.roleId} className="text-center">
                      <input
                        type="checkbox"
                        checked={checked}
                        aria-label={label}
                        onChange={() => toggle(role.roleId, permission.id)}
                        className="h-4 w-4 accent-[var(--flit-action)]"
                      />
                    </td>
                  );
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
