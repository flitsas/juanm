import type { OcrBufferFieldErrors, OcrBufferForm } from "./ocr.types";

export function validateOcrBufferForm(form: OcrBufferForm): OcrBufferFieldErrors {
  const errors: OcrBufferFieldErrors = {};

  if (!form.numeroComparendo.trim()) {
    errors.numeroComparendo = "El número de comparendo es obligatorio.";
  }

  if (!form.estado.trim()) {
    errors.estado = "El estado es obligatorio.";
  }

  return errors;
}

export function hasOcrBufferErrors(errors: OcrBufferFieldErrors): boolean {
  return Object.keys(errors).length > 0;
}
