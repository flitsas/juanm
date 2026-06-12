export type PlantillaFieldType = "text" | "number" | "choice";

export type PdfTemplateField = {
  id: string;
  acroformName: string;
  fieldType: PlantillaFieldType;
  systemVariable: string | null;
  choiceOptions: string[] | null;
  sortOrder: number;
};

export type PdfTemplateSummary = {
  id: string;
  name: string;
  version: number;
  isActive: boolean;
  fieldCount: number;
};

export type PdfTemplateDetail = {
  id: string;
  name: string;
  version: number;
  isActive: boolean;
  description: string | null;
  fields: PdfTemplateField[];
};

export type PdfTemplateListResponse = {
  items: PdfTemplateSummary[];
};

export type UploadPdfTemplateResponse = {
  id: string;
  name: string;
  version: number;
  detectedFields: PdfTemplateField[];
};

export type SystemVariable = {
  key: string;
  label: string;
  source: string;
  dataType: PlantillaFieldType;
};

export type SystemVariableListResponse = {
  items: SystemVariable[];
};

export type FieldMappingInput = {
  fieldId: string;
  fieldType: PlantillaFieldType;
  systemVariable: string;
  choiceOptions?: string[] | null;
};

export type UpdateFieldMappingsRequest = {
  fields: FieldMappingInput[];
};

export const PLANTILLA_CATEGORIES = ["Derecho de Petición", "Correspondencia legal"] as const;

export const FIELD_TYPE_LABELS: Record<PlantillaFieldType, string> = {
  text: "Texto",
  number: "Número",
  choice: "Lista / selección",
};

export type GenerateDerechoPeticionResponse = {
  derechoPeticionId: string;
  comparendoId: string;
  templateId: string;
  templateVersion: number;
  estado: string;
  outputStorageKey: string;
  generatedAt: string;
};

export type PlantillasApiErrorBody = {
  code?: string;
  message?: string;
};
