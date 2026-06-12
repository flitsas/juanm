"use client";

import { Button } from "primereact/button";
import { useState } from "react";
import { PageHeaderCard } from "@/components/shell/PageHeaderCard";
import {
  usePdfTemplate,
  usePdfTemplates,
  usePlantillaMutations,
  useSystemVariables,
} from "../api/use-plantillas";
import type { PdfTemplateSummary } from "../lib/plantillas.types";
import { TemplateGenerationDialog } from "./TemplateGenerationDialog";
import { TemplateMappingPanel } from "./TemplateMappingPanel";
import { TemplatesCatalogGrid } from "./TemplatesCatalogGrid";
import { TemplateUploadPanel } from "./TemplateUploadPanel";

type EditorMode = "upload" | "mapping";

export function GdcPlantillasPage() {
  const templatesQuery = usePdfTemplates();
  const systemVariablesQuery = useSystemVariables();
  const { upload, saveMappings, activate } = usePlantillaMutations();

  const [editorMode, setEditorMode] = useState<EditorMode | null>(null);
  const [editingTemplateId, setEditingTemplateId] = useState<string | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [localFields, setLocalFields] = useState<
    import("../lib/plantillas.types").PdfTemplateField[]
  >([]);
  const [localName, setLocalName] = useState("");
  const [generationTemplate, setGenerationTemplate] = useState<PdfTemplateSummary | null>(null);

  const detailQuery = usePdfTemplate(editingTemplateId);

  const templates = templatesQuery.data?.items ?? [];
  const showEditor = editorMode !== null;

  const openNewTemplate = () => {
    setEditorMode("upload");
    setEditingTemplateId(null);
    setLocalFields([]);
    setLocalName("");
    setUploadError(null);
  };

  const openConfigureTemplate = (template: PdfTemplateSummary) => {
    setEditorMode("mapping");
    setEditingTemplateId(template.id);
    setLocalFields([]);
    setUploadError(null);
  };

  const openGenerationDialog = (template: PdfTemplateSummary) => {
    setGenerationTemplate(template);
  };

  const closeEditor = () => {
    setEditorMode(null);
    setEditingTemplateId(null);
    setLocalFields([]);
    setUploadError(null);
  };

  const handleUpload = async (file: File, name?: string) => {
    setUploadError(null);
    try {
      const response = await upload.mutateAsync({ file, name });
      setEditingTemplateId(response.id);
      setLocalName(response.name);
      setLocalFields(response.detectedFields);
      setEditorMode("mapping");
    } catch (err) {
      setUploadError(err instanceof Error ? err.message : "No se pudo cargar el PDF.");
    }
  };

  const mappingFields = localFields.length > 0 ? localFields : (detailQuery.data?.fields ?? []);
  const mappingName = localName || detailQuery.data?.name || "Plantilla";

  if (showEditor && editorMode === "upload") {
    return (
      <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="gdc-plantillas-page">
        <PageHeaderCard
          title="Plantillas documentales"
          subtitle="GDC · generador de derechos de petición y correspondencia legal"
        />
        <TemplateUploadPanel
          uploading={upload.isPending}
          error={uploadError}
          onUpload={(file, name) => void handleUpload(file, name)}
          onCancel={closeEditor}
        />
      </div>
    );
  }

  if (showEditor && editorMode === "mapping") {
    if (detailQuery.isLoading && localFields.length === 0) {
      return (
        <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="gdc-plantillas-page">
          <div
            className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
            role="status"
          >
            Cargando tags y variables del sistema…
          </div>
        </div>
      );
    }

    return (
      <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="gdc-plantillas-page">
        <PageHeaderCard
          title="Plantillas documentales"
          subtitle="Configure el mapeo de variables del sistema"
        />
        <TemplateMappingPanel
          templateName={mappingName}
          fields={mappingFields}
          systemVariables={systemVariablesQuery.data?.items ?? []}
          saving={saveMappings.isPending}
          activating={activate.isPending}
          onSave={async (mappings) => {
            if (!editingTemplateId) return;
            const detail = await saveMappings.mutateAsync({
              templateId: editingTemplateId,
              body: { fields: mappings },
            });
            setLocalFields(detail.fields);
          }}
          onActivate={async () => {
            if (!editingTemplateId) return;
            await activate.mutateAsync(editingTemplateId);
            closeEditor();
          }}
          onCancel={closeEditor}
        />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-[1400px] space-y-6 p-6" data-testid="gdc-plantillas-page">
      <PageHeaderCard
        title="Plantillas documentales"
        subtitle="GDC · catálogo de plantillas PDF y mapeo AcroForm"
        actions={
          <Button
            type="button"
            label="Nueva plantilla"
            className="flit-btn-primary"
            onClick={openNewTemplate}
            data-testid="gdc-new-template-btn"
          />
        }
      />

      {templatesQuery.isLoading ? (
        <div
          className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
          role="status"
        >
          Cargando plantillas…
        </div>
      ) : null}

      {!templatesQuery.isLoading && templatesQuery.isError ? (
        <div
          className="rounded-2xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-6 text-sm text-[var(--alert)]"
          role="alert"
        >
          No se pudieron cargar las plantillas.
        </div>
      ) : null}

      {!templatesQuery.isLoading && !templatesQuery.isError && templates.length === 0 ? (
        <div
          className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-12 text-center"
          data-testid="gdc-templates-empty"
        >
          <p className="text-sm font-medium text-[var(--deep)]">Sin plantillas registradas</p>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">
            Cargue un PDF con campos AcroForm para iniciar el flujo de mapeo.
          </p>
        </div>
      ) : null}

      {!templatesQuery.isLoading && !templatesQuery.isError && templates.length > 0 ? (
        <TemplatesCatalogGrid
          templates={templates}
          onUseTemplate={openGenerationDialog}
          onConfigureTemplate={openConfigureTemplate}
        />
      ) : null}

      <TemplateGenerationDialog
        template={generationTemplate}
        visible={generationTemplate !== null}
        onHide={() => setGenerationTemplate(null)}
      />
    </div>
  );
}
