"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { getSession } from "@/features/auth/lib/session";

export default function HomePage() {
  const router = useRouter();

  useEffect(() => {
    router.replace(getSession() ? "/dashboard" : "/login");
  }, [router]);

  return null;
}
