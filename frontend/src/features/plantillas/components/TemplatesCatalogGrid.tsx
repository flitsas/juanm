"use client";

import { Button } from "primereact/button";
import type { PdfTemplateSummary } from "../lib/plantillas.types";

type TemplatesCatalogGridProps = {
  templates: PdfTemplateSummary[];
  onUseTemplate: (template: PdfTemplateSummary) => void;
};

export function TemplatesCatalogGrid({ templates, onUseTemplate }: TemplatesCatalogGridProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3" data-testid="gdc-templates-catalog">
      {templates.map((template) => (
        <article
          key={template.id}
          className="flit-card flex flex-col gap-3 rounded-2xl border border-[var(--border)] bg-[var(--card)] p-5 shadow-sm"
        >
          <div className="flex items-start justify-between gap-2">
            <span className="rounded-full bg-[var(--table-head)] px-3 py-1 text-xs font-semibold text-[var(--deep)]">
              Derecho de Petición
            </span>
            <span
              className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                template.isActive
                  ? "bg-[var(--tech)]/15 text-[var(--deep)]"
                  : "bg-[var(--muted)] text-[var(--muted-foreground)]"
              }`}
            >
              {template.isActive ? "Activa" : "Inactiva"}
            </span>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-[var(--deep)]">{template.name}</h3>
            <p className="mt-1 text-sm text-[var(--muted-foreground)]">
              {template.fieldCount} campos AcroForm · versión v{template.version}
            </p>
          </div>
          <div className="mt-auto flex justify-end">
            <Button
              type="button"
              label="Usar plantilla"
              className="flit-btn-primary"
              onClick={() => onUseTemplate(template)}
              data-testid={`gdc-use-template-${template.id}`}
            />
          </div>
        </article>
      ))}
    </div>
  );
}
