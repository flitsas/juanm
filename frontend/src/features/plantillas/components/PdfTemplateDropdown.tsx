"use client";

import Link from "next/link";
import { Dropdown } from "primereact/dropdown";
import { useMemo } from "react";
import { usePdfTemplates } from "../api/use-plantillas";
import { buildPdfTemplateSelectOptions } from "../lib/template-select-options";

type PdfTemplateDropdownProps = {
  value: string;
  onChange: (templateId: string) => void;
  className?: string;
  inputId?: string;
  testId?: string;
};

export function PdfTemplateDropdown({
  value,
  onChange,
  className,
  inputId,
  testId = "pdf-template-dropdown",
}: PdfTemplateDropdownProps) {
  const templatesQuery = usePdfTemplates();

  const options = useMemo(
    () => buildPdfTemplateSelectOptions(templatesQuery.data?.items ?? [], value),
    [templatesQuery.data?.items, value],
  );

  if (templatesQuery.isLoading) {
    return (
      <p className="text-sm text-[var(--muted-foreground)]" role="status">
        Cargando plantillas GDC…
      </p>
    );
  }

  if (templatesQuery.isError) {
    return (
      <p className="text-sm text-[var(--alert)]" role="alert">
        No se pudieron cargar las plantillas PDF. Verifique la conexión con GDC.
      </p>
    );
  }

  if (options.length <= 1) {
    return (
      <p className="text-sm text-[var(--muted-foreground)]">
        No hay plantillas activas. Suba y active una en{" "}
        <Link href="/gdc" className="font-medium text-[var(--tech)] underline">
          GDC Plantillas
        </Link>
        .
      </p>
    );
  }

  return (
    <Dropdown
      inputId={inputId}
      value={value}
      options={options}
      onChange={(e) => onChange((e.value as string) ?? "")}
      className={className}
      placeholder="Seleccione plantilla PDF"
      data-testid={testId}
    />
  );
}
