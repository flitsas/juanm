import type { OcrBufferForm, OcrItemResponse } from "./ocr.types";

export function mapOcrItemToForm(
  item: OcrItemResponse,
  fileName: string,
  previewUrl: string | null,
): OcrBufferForm {
  const f = item.fields;
  return {
    itemId: item.itemId,
    fileName,
    previewUrl,
    ocrDiagnostic: f.confidence > 0 ? "" : (f.rawText ?? ""),
    numeroComparendo: f.numeroComparendo ?? "",
    estado: "Pendiente",
    infractorNombre: f.infractor ?? "",
    documento: f.documento ?? "",
    placa: f.placa ?? "",
    infraccionCodigo: f.infraccion ?? "",
    fechaComparendo: f.fecha ?? "",
    totalValor: f.valor != null ? String(f.valor) : "",
    confidence: f.confidence,
  };
}
