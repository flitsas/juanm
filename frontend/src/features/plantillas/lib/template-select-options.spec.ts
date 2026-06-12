import { describe, expect, it } from "vitest";
import {
  buildPdfTemplateSelectOptions,
  formatPdfTemplateLabel,
  pickDefaultPdfTemplateId,
  resolvePdfTemplateName,
} from "./template-select-options";
import type { PdfTemplateSummary } from "./plantillas.types";

const active: PdfTemplateSummary = {
  id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  name: "DP Vialix",
  version: 2,
  isActive: true,
  fieldCount: 10,
};

const inactive: PdfTemplateSummary = {
  id: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  name: "DP Legacy",
  version: 1,
  isActive: false,
  fieldCount: 8,
};

describe("buildPdfTemplateSelectOptions", () => {
  it("lista solo plantillas activas por defecto", () => {
    const options = buildPdfTemplateSelectOptions([active, inactive]);
    expect(options).toHaveLength(2);
    expect(options[1]?.value).toBe(active.id);
  });

  it("incluye plantilla inactiva si ya está seleccionada", () => {
    const options = buildPdfTemplateSelectOptions([active, inactive], inactive.id);
    expect(options.map((o) => o.value)).toContain(inactive.id);
    expect(options.find((o) => o.value === inactive.id)?.label).toContain("inactiva");
  });
});

describe("resolvePdfTemplateName", () => {
  it("resuelve nombre legible cuando existe en catálogo", () => {
    expect(resolvePdfTemplateName([active], active.id)).toBe(formatPdfTemplateLabel(active));
  });

  it("recurre a prefijo UUID si no está en catálogo", () => {
    expect(resolvePdfTemplateName([active], "cccccccc-cccc-cccc-cccc-cccccccccccc")).toBe(
      "cccccccc…",
    );
  });
});

describe("pickDefaultPdfTemplateId", () => {
  it("elige la primera plantilla activa", () => {
    expect(pickDefaultPdfTemplateId([inactive, active])).toBe(active.id);
    expect(pickDefaultPdfTemplateId([inactive])).toBe("");
  });
});
