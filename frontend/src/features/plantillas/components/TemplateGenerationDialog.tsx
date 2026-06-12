"use client";

import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { Dropdown } from "primereact/dropdown";
import { useEffect, useMemo, useState } from "react";
import { useComparendosMaestra } from "@/features/dgc/api/use-comparendos-maestra";
import { PlantillasApiError } from "../api/plantillas-api";
import { useDpGeneration } from "../api/use-dp-generation";
import {
  CONTRAVENTOR_BLOCK_MESSAGE,
  hasContraventorIdentificado,
} from "../lib/comparendo-eligibility";
import { createPdfPreviewUrl, downloadPdfBlob } from "../lib/pdf-download";
import type { PdfTemplateSummary } from "../lib/plantillas.types";

type TemplateGenerationDialogProps = {
  template: PdfTemplateSummary | null;
  visible: boolean;
  onHide: () => void;
};

export function TemplateGenerationDialog({
  template,
  visible,
  onHide,
}: TemplateGenerationDialogProps) {
  const comparendosQuery = useComparendosMaestra({
    search: "",
    estado: "",
    secretaria: "",
    fechaDesde: "",
    fechaHasta: "",
    page: 1,
    pageSize: 100,
  });
  const { generate, download } = useDpGeneration();

  const [selectedComparendoId, setSelectedComparendoId] = useState<string | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [generatedDpId, setGeneratedDpId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const comparendos = comparendosQuery.data?.items ?? [];

  const comparendoOptions = useMemo(
    () =>
      comparendos.map((item) => ({
        label: `${item.numeroComparendo} · ${item.placa ?? "sin placa"} · ${item.infractor ?? "—"}`,
        value: item.id,
      })),
    [comparendos],
  );

  const selectedComparendo = comparendos.find((c) => c.id === selectedComparendoId) ?? null;
  const blockedByContraventor =
    selectedComparendo !== null && !hasContraventorIdentificado(selectedComparendo);

  useEffect(() => {
    if (!visible) {
      setSelectedComparendoId(null);
      setGeneratedDpId(null);
      setErrorMessage(null);
      setPreviewUrl((current) => {
        if (current) URL.revokeObjectURL(current);
        return null;
      });
    }
  }, [visible]);

  const handlePreview = async () => {
    if (!template || !selectedComparendoId) return;
    if (blockedByContraventor) {
      setErrorMessage(CONTRAVENTOR_BLOCK_MESSAGE);
      return;
    }

    setErrorMessage(null);
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
    }

    try {
      const generated = await generate.mutateAsync({
        comparendoId: selectedComparendoId,
        templateId: template.id,
      });
      setGeneratedDpId(generated.derechoPeticionId);
      const blob = await download.mutateAsync(generated.derechoPeticionId);
      setPreviewUrl(createPdfPreviewUrl(blob));
    } catch (err) {
      if (err instanceof PlantillasApiError && err.code === "GDC_CONTRAVENTOR_REQUIRED") {
        setErrorMessage(CONTRAVENTOR_BLOCK_MESSAGE);
        return;
      }
      setErrorMessage(err instanceof Error ? err.message : "No se pudo generar la vista previa.");
    }
  };

  const handleDownload = async () => {
    if (!generatedDpId) return;
    if (blockedByContraventor) {
      setErrorMessage(CONTRAVENTOR_BLOCK_MESSAGE);
      return;
    }

    try {
      const blob = await download.mutateAsync(generatedDpId);
      downloadPdfBlob(blob, `derecho-peticion-${generatedDpId}.pdf`);
    } catch (err) {
      if (err instanceof PlantillasApiError && err.code === "GDC_CONTRAVENTOR_REQUIRED") {
        setErrorMessage(CONTRAVENTOR_BLOCK_MESSAGE);
        return;
      }
      setErrorMessage(err instanceof Error ? err.message : "No se pudo descargar el PDF.");
    }
  };

  return (
    <Dialog
      visible={visible}
      onHide={onHide}
      header={`Generar documento — ${template?.name ?? ""}`}
      className="gdc-generation-dialog"
      style={{ width: "min(960px, 96vw)" }}
      data-testid="gdc-generation-dialog"
      modal
    >
      <div className="space-y-4">
        <p className="text-sm text-[var(--muted-foreground)]">
          Seleccione el comparendo destino. La vista previa compila el PDF con los datos
          transaccionales del comparendo y del contraventor.
        </p>

        <div className="space-y-2">
          <label className="text-sm font-medium text-[var(--deep)]" htmlFor="gdc-comparendo-select">
            Comparendo destino
          </label>
          <Dropdown
            inputId="gdc-comparendo-select"
            value={selectedComparendoId}
            options={comparendoOptions}
            onChange={(e) => {
              setSelectedComparendoId(e.value as string);
              setErrorMessage(null);
              setGeneratedDpId(null);
              if (previewUrl) {
                URL.revokeObjectURL(previewUrl);
                setPreviewUrl(null);
              }
            }}
            placeholder="Seleccionar comparendo"
            className="w-full"
            filter
            data-testid="gdc-comparendo-select"
          />
        </div>

        {blockedByContraventor ? (
          <div
            className="rounded-xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-4 text-sm text-[var(--alert)]"
            role="alert"
            data-testid="gdc-contraventor-block"
          >
            {CONTRAVENTOR_BLOCK_MESSAGE}
          </div>
        ) : null}

        {errorMessage && !blockedByContraventor ? (
          <div
            className="rounded-xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-4 text-sm text-[var(--alert)]"
            role="alert"
          >
            {errorMessage}
          </div>
        ) : null}

        <div className="flex flex-wrap gap-2">
          <Button
            type="button"
            label="Generar vista previa"
            className="flit-btn-primary"
            disabled={!selectedComparendoId || blockedByContraventor || generate.isPending}
            loading={generate.isPending || download.isPending}
            onClick={() => void handlePreview()}
            data-testid="gdc-generate-preview-btn"
          />
          <Button
            type="button"
            label="Descargar PDF"
            className="flit-btn-secondary"
            disabled={!generatedDpId || blockedByContraventor}
            loading={download.isPending}
            onClick={() => void handleDownload()}
            data-testid="gdc-download-pdf-btn"
          />
        </div>

        {previewUrl ? (
          <div
            className="overflow-hidden rounded-2xl border border-[var(--border)] bg-[var(--muted)]/20"
            data-testid="gdc-pdf-preview"
          >
            <iframe
              title="Vista previa del derecho de petición"
              src={previewUrl}
              className="h-[min(70vh,640px)] w-full bg-white"
            />
          </div>
        ) : (
          <div
            className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--card)] p-10 text-center text-sm text-[var(--muted-foreground)]"
            data-testid="gdc-pdf-preview-empty"
          >
            La vista previa del documento legal aparecerá aquí tras generar.
          </div>
        )}
      </div>
    </Dialog>
  );
}
