"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { isSuperAdmin } from "../lib/roles";
import { getSession } from "../lib/session";

export function AdminNavLink() {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    setVisible(isSuperAdmin(getSession()));
  }, []);

  if (!visible) return null;

  return (
    <Link
      href="/admin"
      className="text-sm font-medium text-[var(--flit-text-brand)] underline-offset-2 hover:underline"
    >
      Consola Super Admin
    </Link>
  );
}
