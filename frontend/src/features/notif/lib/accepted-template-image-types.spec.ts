import { describe, expect, it } from "vitest";
import { isAcceptedTemplateImage } from "./accepted-template-image-types";

describe("isAcceptedTemplateImage", () => {
  it("acepta PNG y JPG", () => {
    expect(
      isAcceptedTemplateImage(new File(["x"], "banner.png", { type: "image/png" })),
    ).toBe(true);
    expect(
      isAcceptedTemplateImage(new File(["x"], "pie.jpg", { type: "image/jpeg" })),
    ).toBe(true);
  });

  it("rechaza PDF y GIF", () => {
    expect(
      isAcceptedTemplateImage(new File(["x"], "doc.pdf", { type: "application/pdf" })),
    ).toBe(false);
    expect(
      isAcceptedTemplateImage(new File(["x"], "anim.gif", { type: "image/gif" })),
    ).toBe(false);
  });
});
