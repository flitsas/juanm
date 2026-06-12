"use client";

import { ContactsSection } from "./ContactsSection";
import { LogsSection } from "./LogsSection";

export function LogsAndContactsSection() {
  return (
    <div className="space-y-10" data-testid="reglas-logs-contacts-section">
      <LogsSection />
      <div>
        <h2 className="mb-4 text-lg font-semibold text-[var(--deep)]">Contactos de secretaría</h2>
        <ContactsSection />
      </div>
    </div>
  );
}
