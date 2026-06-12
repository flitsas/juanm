"use client";

import { useLogout } from "../hooks/use-logout";

export function LogoutButton() {
  const { onLogout, loading } = useLogout();

  return (
    <button
      type="button"
      onClick={onLogout}
      disabled={loading}
      className="flit-gradient-btn h-10 px-6 text-sm"
      aria-label="Cerrar sesión"
    >
      {loading ? "Cerrando…" : "Cerrar sesión"}
    </button>
  );
}
