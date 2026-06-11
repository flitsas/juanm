export type OcrExtractedFields = {
  numeroComparendo: string | null;
  placa: string | null;
  documento: string | null;
  infractor: string | null;
  fecha: string | null;
  valor: number | null;
  infraccion: string | null;
  confidence: number;
  rawText: string;
};

export type OcrItemResponse = {
  itemId: string;
  loteId: string;
  estado: string;
  archivoUri: string;
  fields: OcrExtractedFields;
};

export type OcrLoteUploadResult = {
  loteId: string;
  items: OcrItemResponse[];
};

export type OcrBufferForm = {
  itemId: string;
  fileName: string;
  previewUrl: string | null;
  ocrDiagnostic: string;
  numeroComparendo: string;
  estado: string;
  infractorNombre: string;
  documento: string;
  placa: string;
  infraccionCodigo: string;
  fechaComparendo: string;
  totalValor: string;
  confidence: number;
};

export type OcrBufferFieldErrors = Partial<
  Record<
    | "numeroComparendo"
    | "estado"
    | "infractorNombre"
    | "documento"
    | "placa"
    | "totalValor",
    string
  >
>;
