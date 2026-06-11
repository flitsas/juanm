import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ImageAssetDropzone } from "./ImageAssetDropzone";

describe("ImageAssetDropzone", () => {
  it("rechaza archivos que no son PNG o JPG", () => {
    const onUpload = vi.fn();
    render(<ImageAssetDropzone label="Banner" assetUrl="" onUpload={onUpload} testId="drop" />);

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    const file = new File(["x"], "doc.pdf", { type: "application/pdf" });
    fireEvent.change(input, { target: { files: [file] } });

    expect(screen.getByText("Solo se permiten imágenes PNG o JPG.")).toBeTruthy();
    expect(onUpload).not.toHaveBeenCalled();
  });
});
