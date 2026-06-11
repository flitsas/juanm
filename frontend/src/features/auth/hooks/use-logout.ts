"use client";

import { useRouter } from "next/navigation";
import { useCallback, useState } from "react";
import { logout } from "../api/auth-api";
import { clearSession, getSession } from "../lib/session";

export function useLogout() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);

  const onLogout = useCallback(async () => {
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
  }, [router]);

  return { onLogout, loading };
}
