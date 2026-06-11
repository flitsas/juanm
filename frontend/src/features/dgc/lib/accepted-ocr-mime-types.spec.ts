import { describe, expect, it } from "vitest";
import { isAcceptedOcrFile, OCR_ACCEPT_ATTRIBUTE } from "./accepted-ocr-mime-types";

describe("accepted OCR mime types", () => {
  it("acepta PDF, PNG y JPG por extensión", () => {
    expect(isAcceptedOcrFile(new File(["x"], "doc.pdf"))).toBe(true);
    expect(isAcceptedOcrFile(new File(["x"], "img.png", { type: "image/png" }))).toBe(true);
    expect(isAcceptedOcrFile(new File(["x"], "photo.jpg", { type: "image/jpeg" }))).toBe(true);
  });

  it("rechaza formatos no soportados", () => {
    expect(isAcceptedOcrFile(new File(["x"], "data.csv"))).toBe(false);
  });

  it("expone atributo accept para el input", () => {
    expect(OCR_ACCEPT_ATTRIBUTE).toContain(".pdf");
    expect(OCR_ACCEPT_ATTRIBUTE).toContain(".png");
    expect(OCR_ACCEPT_ATTRIBUTE).toContain(".jpg");
  });
});
