"use client";

import { useRouter } from "next/navigation";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { useState } from "react";
import { login } from "../api/auth-api";
import { setSession } from "../lib/session";
import { GradientButton } from "./gradient-button";

export function LoginForm() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      const session = await login({ email, password });
      setSession(session);
      router.push("/");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Credenciales inválidas.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={onSubmit} noValidate>
      <h1 className="text-3xl font-bold text-[var(--flit-text-primary)]">Bienvenido</h1>
      <p className="mt-1 text-sm font-light text-[var(--flit-text-secondary)]">
        Accede a tu panel de gestión integral
      </p>

      {error ? (
        <p
          role="alert"
          className="mt-6 rounded-[var(--flit-radius-input)] border border-[var(--flit-state-danger)] bg-[var(--flit-bg-danger-soft)] px-4 py-3 text-sm text-[var(--flit-state-danger)]"
        >
          {error}
        </p>
      ) : null}

      <div className="mt-8 space-y-4">
        <div className="space-y-1.5">
          <label htmlFor="email" className="text-xs font-medium text-[var(--flit-text-primary)]">
            Usuario o correo
          </label>
          <InputText
            id="email"
            name="email"
            type="email"
            autoComplete="username"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="flit-field-input w-full"
            placeholder="tu@empresa.co"
          />
        </div>

        <div className="space-y-1.5">
          <label
            htmlFor="login-password"
            className="text-xs font-medium text-[var(--flit-text-primary)]"
          >
            Contraseña
          </label>
          <Password
            inputId="login-password"
            name="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            toggleMask
            feedback={false}
            className="flit-password w-full"
            inputClassName="flit-field-input w-full"
            placeholder="••••••••"
          />
        </div>
        <div className="text-right">
          <button
            type="button"
            className="text-xs font-medium text-[var(--flit-action)] hover:underline"
          >
            Olvidé mi contraseña
          </button>
        </div>
      </div>

      <GradientButton
        type="submit"
        disabled={loading}
        className="mt-6 h-12 text-base"
        aria-busy={loading}
      >
        {loading ? "Ingresando…" : "Ingresar"}
      </GradientButton>
    </form>
  );
}
