"use client";

import { NotifAdminPanel } from "./NotifAdminPanel";

/** @deprecated Usar UnifiedAdminPage en /admin dentro del shell. */
export function NotifAdminPage() {
  return (
    <div className="mx-auto max-w-[1200px] space-y-6 p-6">
      <NotifAdminPanel />
    </div>
  );
}
