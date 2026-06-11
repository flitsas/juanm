"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { activateAccount } from "../api/auth-api";
import { GradientButton } from "./gradient-button";

export function ActivateAccountForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get("token") ?? "";

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  if (!token) {
    return (
      <div>
        <h1 className="text-3xl font-bold text-[var(--flit-text-primary)]">Activar cuenta</h1>
        <p
          role="alert"
          className="mt-6 rounded-[var(--flit-radius-input)] border border-[var(--flit-state-danger)] bg-[#FFF5F3] px-4 py-3 text-sm text-[var(--flit-state-danger)]"
        >
          El enlace de activación no es válido o falta el token.
        </p>
        <p className="mt-4 text-sm text-[var(--flit-text-secondary)]">
          Solicita una nueva invitación al administrador de tu compañía.
        </p>
        <Link
          href="/login"
          className="mt-6 inline-block text-sm font-semibold text-[var(--flit-brand-primary)] underline"
        >
          Ir a iniciar sesión
        </Link>
      </div>
    );
  }

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    setLoading(true);

    try {
      await activateAccount({ token, password });
      router.push("/login?activated=1");
    } catch (err) {
      const status = (err as Error & { status?: number }).status;
      if (status === 410) {
        setError("El enlace de activación ha expirado. Solicita una nueva invitación.");
      } else {
        setError(err instanceof Error ? err.message : "No se pudo activar la cuenta.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={onSubmit} noValidate>
      <h1 className="text-3xl font-bold text-[var(--flit-text-primary)]">Activar cuenta</h1>
      <p className="mt-1 text-sm font-light text-[var(--flit-text-secondary)]">
        Define tu contraseña para completar el registro
      </p>

      {error ? (
        <p
          role="alert"
          className="mt-6 rounded-[var(--flit-radius-input)] border border-[var(--flit-state-danger)] bg-[#FFF5F3] px-4 py-3 text-sm text-[var(--flit-state-danger)]"
        >
          {error}
        </p>
      ) : null}

      <div className="mt-8 space-y-4">
        <div className="space-y-1.5">
          <label
            htmlFor="activate-password"
            className="text-xs font-semibold text-[var(--flit-text-primary)]"
          >
            Nueva contraseña
          </label>
          <input
            id="activate-password"
            name="password"
            type="password"
            autoComplete="new-password"
            required
            minLength={8}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="flit-input"
            placeholder="Mínimo 8 caracteres"
          />
        </div>

        <div className="space-y-1.5">
          <label
            htmlFor="activate-confirm-password"
            className="text-xs font-semibold text-[var(--flit-text-primary)]"
          >
            Confirmar contraseña
          </label>
          <input
            id="activate-confirm-password"
            name="confirmPassword"
            type="password"
            autoComplete="new-password"
            required
            minLength={8}
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            className="flit-input"
            placeholder="Repite tu contraseña"
          />
        </div>
      </div>

      <GradientButton
        type="submit"
        disabled={loading}
        className="mt-6 h-12 text-base"
        aria-busy={loading}
      >
        {loading ? "Activando…" : "Activar cuenta"}
      </GradientButton>
    </form>
  );
}
