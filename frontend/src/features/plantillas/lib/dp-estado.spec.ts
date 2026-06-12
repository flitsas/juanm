import { describe, expect, it } from "vitest";
import { DP_ESTADO_BADGE_CLASS, formatDpEstadoLabel } from "./dp-estado";

describe("dp-estado", () => {
  it("formatea etiquetas RF05 en español", () => {
    expect(formatDpEstadoLabel("NoEnviado")).toBe("No enviado");
    expect(formatDpEstadoLabel("ConRespuesta")).toBe("Con respuesta");
  });

  it("define clases de badge para cada estado", () => {
    expect(DP_ESTADO_BADGE_CLASS.NoEnviado).toContain("var(--muted)");
    expect(DP_ESTADO_BADGE_CLASS.Enviado).toContain("var(--tech)");
    expect(DP_ESTADO_BADGE_CLASS.SinRespuesta).toContain("var(--amber-acc)");
    expect(DP_ESTADO_BADGE_CLASS.ConRespuesta).toContain("var(--action)");
  });
});
