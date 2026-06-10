"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { getSession } from "@/features/auth/lib/session";

export default function HomePage() {
  const router = useRouter();

  useEffect(() => {
    router.replace(getSession() ? "/dashboard" : "/login");
  }, [router]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-[var(--flit-bg-app)] text-[var(--flit-text-secondary)]">
      Redirigiendo…
    </div>
  );
}
