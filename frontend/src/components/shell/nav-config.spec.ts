import { describe, expect, it } from "vitest";
import { resolveActiveNavId, SHELL_NAV_ITEMS } from "./nav-config";

describe("nav-config", () => {
  it("incluye Comparendos con ruta /dgc", () => {
    const dgc = SHELL_NAV_ITEMS.find((item) => item.id === "dgc");
    expect(dgc?.label).toBe("Comparendos");
    expect(dgc?.href).toBe("/dgc");
    expect(dgc?.enabled).toBe(true);
  });

  it("resuelve activeId para rutas DGC", () => {
    expect(resolveActiveNavId("/dgc")).toBe("dgc");
    expect(resolveActiveNavId("/")).toBe("dashboard");
  });

  it("habilita Plantillas GDC en /gdc", () => {
    const gdcPlantillas = SHELL_NAV_ITEMS.find((item) => item.id === "gdc");
    expect(gdcPlantillas?.label).toBe("Plantillas");
    expect(gdcPlantillas?.href).toBe("/gdc");
    expect(gdcPlantillas?.enabled).toBe(true);
    expect(resolveActiveNavId("/gdc")).toBe("gdc");
  });

  it("habilita Admin unificado en /admin", () => {
    const admin = SHELL_NAV_ITEMS.find((item) => item.id === "admin");
    expect(admin?.enabled).toBe(true);
    expect(admin?.href).toBe("/admin");
    expect(resolveActiveNavId("/admin")).toBe("admin");
    expect(resolveActiveNavId("/admin/notificaciones")).toBe("admin");
  });
});
