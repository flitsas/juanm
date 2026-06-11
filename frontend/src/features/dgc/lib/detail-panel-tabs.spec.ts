import { describe, expect, it } from "vitest";
import { DETAIL_PANEL_TABS } from "./detail-panel.types";

describe("DETAIL_PANEL_TABS", () => {
  it("define pestañas Detalle, Contraventor y Log de correos", () => {
    const labels = DETAIL_PANEL_TABS.map((t) => t.label);
    expect(labels).toContain("Detalle");
    expect(labels).toContain("Contraventor");
    expect(labels).toContain("Log de correos");
    expect(DETAIL_PANEL_TABS).toHaveLength(3);
  });
});
