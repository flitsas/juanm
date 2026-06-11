import { describe, expect, it } from "vitest";
import { composeTemplatePreviewHtml } from "./template-preview";

describe("composeTemplatePreviewHtml", () => {
  it("integra banner, cuerpo y pie en el preview", () => {
    const html = composeTemplatePreviewHtml(
      "<p>Hola {{infractor}}</p>",
      "https://cdn.test/banner.png",
      "https://cdn.test/footer.jpg",
      { infractor: "Ana" },
    );
    expect(html).toContain("banner.png");
    expect(html).toContain("footer.jpg");
    expect(html).toContain("Hola Ana");
  });
});
