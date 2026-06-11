"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { logout } from "../api/auth-api";
import { clearSession, getSession } from "../lib/session";

export function LogoutButton() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const onLogout = async () => {
    setLoading(true);
    const session = getSession();
    try {
      if (session?.accessToken) {
        await logout(session.accessToken);
      }
    } catch {
      // Limpia sesión local aunque falle el API (token ya revocado, etc.)
    } finally {
      clearSession();
      setLoading(false);
      router.replace("/login");
    }
  };

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
