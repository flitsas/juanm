"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { FlitModal } from "@/components/flit/modal-form";
import { confirmOcrItem, uploadOcrLote } from "../api/ocr-api";
import { isAcceptedOcrFile, OCR_ACCEPT_ATTRIBUTE } from "../lib/accepted-ocr-mime-types";
import { mapOcrItemToForm } from "../lib/map-ocr-item-to-form";
import type { OcrBufferFieldErrors, OcrBufferForm } from "../lib/ocr.types";
import { hasOcrBufferErrors, validateOcrBufferForm } from "../lib/ocr-buffer-validation";
import { DgcOcrBufferForm } from "./DgcOcrBufferForm";

type DgcOcrUploadDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirmed?: () => void;
};

type BufferEntry = {
  form: OcrBufferForm;
  errors: OcrBufferFieldErrors;
  confirming: boolean;
  confirmed: boolean;
};

export function DgcOcrUploadDialog({ open, onOpenChange, onConfirmed }: DgcOcrUploadDialogProps) {
  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [entries, setEntries] = useState<BufferEntry[]>([]);
  const [dragOver, setDragOver] = useState(false);
  const entriesRef = useRef(entries);
  entriesRef.current = entries;

  const reset = useCallback(() => {
    for (const entry of entriesRef.current) {
      if (entry.form.previewUrl) URL.revokeObjectURL(entry.form.previewUrl);
    }
    setEntries((prev) => (prev.length === 0 ? prev : []));
    setUploadError((prev) => (prev === null ? prev : null));
    setUploading((prev) => (prev === false ? prev : false));
  }, []);

  useEffect(() => {
    if (!open) reset();
  }, [open, reset]);

  const processFiles = async (fileList: FileList | File[]) => {
    const files = [...fileList].filter(isAcceptedOcrFile);
    if (files.length === 0) {
      setUploadError("Solo se permiten archivos PDF, PNG o JPG.");
      return;
    }

    setUploading(true);
    setUploadError(null);
    try {
      const result = await uploadOcrLote(files);
      const previewByIndex = files.map((f) =>
        f.type.startsWith("image/") ? URL.createObjectURL(f) : null,
      );
      const newEntries = result.items.map((item, index) => ({
        form: mapOcrItemToForm(
          item,
          files[index]?.name ?? item.archivoUri,
          previewByIndex[index] ?? null,
        ),
        errors: {} as OcrBufferFieldErrors,
        confirming: false,
        confirmed: false,
      }));
      setEntries(newEntries);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Error desconocido";
      setUploadError(
        message.includes("401") || message.includes("403")
          ? "Sesión inválida o tenant incorrecto. Inicie sesión nuevamente."
          : "No se pudo procesar la carga OCR. Verifique el API y python-ml.",
      );
    } finally {
      setUploading(false);
    }
  };

  const handleConfirm = async (index: number) => {
    const entry = entries[index];
    if (!entry) return;

    const errors = validateOcrBufferForm(entry.form);
    if (hasOcrBufferErrors(errors)) {
      setEntries((prev) => prev.map((e, i) => (i === index ? { ...e, errors } : e)));
      return;
    }

    setEntries((prev) =>
      prev.map((e, i) => (i === index ? { ...e, confirming: true, errors: {} } : e)),
    );

    try {
      await confirmOcrItem(entry.form);
      setEntries((prev) =>
        prev.map((e, i) => (i === index ? { ...e, confirming: false, confirmed: true } : e)),
      );
      onConfirmed?.();
    } catch (err) {
      const message = err instanceof Error ? err.message : "Error al confirmar";
      setEntries((prev) =>
        prev.map((e, i) =>
          i === index
            ? {
                ...e,
                confirming: false,
                errors: { numeroComparendo: message },
              }
            : e,
        ),
      );
    }
  };

  return (
    <FlitModal
      open={open}
      onClose={() => onOpenChange(false)}
      title="Cargar comparendos (OCR)"
      titleId="ocr-upload-title"
      testId="dgc-ocr-upload-dialog"
      size="xl"
    >
      {/* biome-ignore lint/a11y/noStaticElementInteractions: zona de drop para input file oculto */}
      <div
        className={`rounded-2xl border-2 border-dashed p-10 text-center transition ${
          dragOver
            ? "border-[var(--flit-brand)] bg-[var(--flit-brand)]/5"
            : "border-[var(--flit-border-input)] bg-[var(--flit-bg-muted)]/40"
        }`}
        onDragOver={(e) => {
          e.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(e) => {
          e.preventDefault();
          setDragOver(false);
          void processFiles(e.dataTransfer.files);
        }}
        data-testid="ocr-dropzone"
      >
        <p className="text-sm font-medium text-[var(--flit-text-primary)]">
          Arrastra archivos PDF, PNG o JPG aquí
        </p>
        <p className="mt-1 text-xs text-[var(--flit-text-secondary)]">
          o selecciónalos desde tu equipo
        </p>
        <label className="mt-4 inline-block cursor-pointer rounded-full bg-[var(--flit-brand)] px-5 py-2.5 text-sm font-medium text-white transition hover:opacity-90">
          Seleccionar archivos
          <input
            type="file"
            accept={OCR_ACCEPT_ATTRIBUTE}
            multiple
            className="sr-only"
            onChange={(e) => {
              if (e.target.files) void processFiles(e.target.files);
            }}
          />
        </label>
      </div>

      {uploading ? (
        <p className="mt-4 text-center text-sm text-[var(--flit-text-secondary)]" role="status">
          Procesando OCR…
        </p>
      ) : null}

      {uploadError ? (
        <p className="mt-4 text-center text-sm text-[var(--flit-state-danger)]" role="alert">
          {uploadError}
        </p>
      ) : null}

      {entries.length > 0 ? (
        <div className="mt-6 space-y-4">
          <h3 className="text-sm font-semibold text-[var(--flit-text-primary)]">
            Buffer de validación ({entries.length})
          </h3>
          {entries.map((entry, index) => (
            <DgcOcrBufferForm
              key={entry.form.itemId}
              form={entry.form}
              errors={entry.errors}
              confirming={entry.confirming}
              confirmed={entry.confirmed}
              onChange={(form) =>
                setEntries((prev) =>
                  prev.map((e, i) => (i === index ? { ...e, form, errors: {} } : e)),
                )
              }
              onConfirm={() => void handleConfirm(index)}
            />
          ))}
        </div>
      ) : null}
    </FlitModal>
  );
}
