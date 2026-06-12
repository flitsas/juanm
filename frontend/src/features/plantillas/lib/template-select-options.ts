import type { PdfTemplateSummary } from "./plantillas.types";

export type PdfTemplateSelectOption = {
  label: string;
  value: string;
};

export function buildPdfTemplateSelectOptions(
  templates: PdfTemplateSummary[],
  selectedId?: string,
): PdfTemplateSelectOption[] {
  const active = templates.filter((t) => t.isActive);
  const selectedInactive =
    selectedId && !active.some((t) => t.id === selectedId)
      ? templates.find((t) => t.id === selectedId)
      : null;

  const items = selectedInactive ? [...active, selectedInactive] : active;

  return [
    { label: "Seleccione plantilla PDF…", value: "" },
    ...items.map((t) => ({
      label: formatPdfTemplateLabel(t),
      value: t.id,
    })),
  ];
}

export function formatPdfTemplateLabel(template: PdfTemplateSummary): string {
  const inactive = template.isActive ? "" : " — inactiva";
  return `${template.name} (v${template.version})${inactive}`;
}

export function resolvePdfTemplateName(
  templates: PdfTemplateSummary[] | undefined,
  templateId: string,
): string {
  const found = templates?.find((t) => t.id === templateId);
  if (found) {
    return formatPdfTemplateLabel(found);
  }
  if (!templateId) {
    return "—";
  }
  return `${templateId.slice(0, 8)}…`;
}

export function pickDefaultPdfTemplateId(templates: PdfTemplateSummary[]): string {
  return templates.find((t) => t.isActive)?.id ?? "";
}
