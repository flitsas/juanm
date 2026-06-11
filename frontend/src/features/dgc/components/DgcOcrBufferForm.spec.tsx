import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import type { OcrBufferForm } from "../lib/ocr.types";
import { DgcOcrBufferForm } from "./DgcOcrBufferForm";

const form: OcrBufferForm = {
  itemId: "item-99",
  fileName: "cmp.png",
  previewUrl: null,
  ocrDiagnostic: "",
  numeroComparendo: "",
  estado: "Pendiente",
  infractorNombre: "",
  documento: "",
  placa: "",
  infraccionCodigo: "",
  fechaComparendo: "",
  totalValor: "",
  confidence: 0.75,
};

describe("DgcOcrBufferForm", () => {
  it("muestra error inline de numeroComparendo", () => {
    render(
      <DgcOcrBufferForm
        form={form}
        errors={{ numeroComparendo: "El número de comparendo es obligatorio." }}
        onChange={vi.fn()}
        onConfirm={vi.fn()}
        confirming={false}
      />,
    );

    expect(screen.getByRole("alert").textContent).toContain("obligatorio");
  });

  it("invoca onConfirm al pulsar guardar", () => {
    const onConfirm = vi.fn();
    render(
      <DgcOcrBufferForm
        form={{ ...form, numeroComparendo: "CMP-200" }}
        errors={{}}
        onChange={vi.fn()}
        onConfirm={onConfirm}
        confirming={false}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /Confirmar comparendo CMP-200/i }));
    expect(onConfirm).toHaveBeenCalledTimes(1);
  });
});
