import { describe, expect, it } from "vitest";
import { formatDpReadonly } from "./format-dp-readonly";

describe("formatDpReadonly", () => {
  it("indica sin trámite cuando dp es vacío", () => {
    expect(formatDpReadonly(null).label).toContain("Sin trámite");
    expect(formatDpReadonly(null).isLink).toBe(false);
  });

  it("expone enlace cuando dp es URL", () => {
    const result = formatDpReadonly("https://tramites.example/dp/123");
    expect(result.isLink).toBe(true);
    expect(result.href).toBe("https://tramites.example/dp/123");
  });

  it("muestra texto plano para referencia sin URL", () => {
    const result = formatDpReadonly("DP-EN-TRAMITE");
    expect(result.isLink).toBe(false);
    expect(result.label).toBe("DP-EN-TRAMITE");
  });
});
