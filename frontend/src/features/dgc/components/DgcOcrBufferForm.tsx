"use client";

import { Button } from "primereact/button";
import { InputNumber } from "primereact/inputnumber";
import { InputText } from "primereact/inputtext";
import { FlitDateField } from "@/components/flit/flit-date-field";
import { FlitSelect } from "@/components/flit/flit-select";
import { FlitFormField } from "@/components/flit/modal-form";
import { COMPARENDO_ESTADO_OPTIONS } from "../lib/comparendo-estado-options";
import { formatCurrencyField, parseCurrencyField } from "../lib/currency-field";
import type { OcrBufferFieldErrors, OcrBufferForm } from "../lib/ocr.types";

type DgcOcrBufferFormProps = {
  form: OcrBufferForm;
  errors: OcrBufferFieldErrors;
  onChange: (next: OcrBufferForm) => void;
  onConfirm: () => void;
  confirming: boolean;
  confirmed?: boolean;
};

export function DgcOcrBufferForm({
  form,
  errors,
  onChange,
  onConfirm,
  confirming,
  confirmed,
}: DgcOcrBufferFormProps) {
  const patch = (partial: Partial<OcrBufferForm>) => onChange({ ...form, ...partial });

  return (
    <article
      className="rounded-2xl border border-[var(--flit-border-input)] bg-[var(--flit-bg-muted)]/10 p-4"
      data-testid={`ocr-buffer-item-${form.itemId}`}
    >
      <div className="flex flex-wrap items-start gap-4">
        {form.previewUrl ? (
          // biome-ignore lint/performance/noImgElement: blob URL local de vista previa OCR
          <img
            src={form.previewUrl}
            alt={`Vista previa ${form.fileName}`}
            className="h-24 w-24 rounded-xl border border-[var(--flit-border-input)] object-cover"
          />
        ) : (
          <div className="flex h-24 w-24 items-center justify-center rounded-xl border border-dashed border-[var(--flit-border-input)] bg-[var(--flit-bg-muted)] text-xs text-[var(--flit-text-secondary)]">
            {form.fileName.endsWith(".pdf") ? "PDF" : "Archivo"}
          </div>
        )}
        <div className="min-w-[200px] flex-1">
          <p className="text-sm font-semibold text-[var(--flit-text-primary)]">{form.fileName}</p>
          <p className="text-xs text-[var(--flit-text-secondary)]">
            Confianza OCR: {(form.confidence * 100).toFixed(0)}%
          </p>
          {form.ocrDiagnostic ? (
            <p className="mt-1 text-xs text-[var(--flit-state-danger)]" role="note">
              {form.ocrDiagnostic}
            </p>
          ) : null}
        </div>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <FlitFormField
          label="No. Comparendo *"
          htmlFor={`ocr-numero-${form.itemId}`}
          error={errors.numeroComparendo}
        >
          <InputText
            id={`ocr-numero-${form.itemId}`}
            value={form.numeroComparendo}
            onChange={(e) => patch({ numeroComparendo: e.target.value })}
            className={`flit-field-input w-full ${errors.numeroComparendo ? "p-invalid" : ""}`}
            aria-invalid={Boolean(errors.numeroComparendo)}
          />
        </FlitFormField>

        <FlitFormField label="Estado *" htmlFor={`ocr-estado-${form.itemId}`} error={errors.estado}>
          <FlitSelect
            inputId={`ocr-estado-${form.itemId}`}
            value={form.estado}
            options={[...COMPARENDO_ESTADO_OPTIONS]}
            onChange={(value) => patch({ estado: value })}
            invalid={Boolean(errors.estado)}
          />
        </FlitFormField>

        <FlitFormField label="Infractor" htmlFor={`ocr-infractor-${form.itemId}`}>
          <InputText
            id={`ocr-infractor-${form.itemId}`}
            value={form.infractorNombre}
            onChange={(e) => patch({ infractorNombre: e.target.value })}
            className="flit-field-input w-full"
          />
        </FlitFormField>

        <FlitFormField label="Documento" htmlFor={`ocr-documento-${form.itemId}`}>
          <InputText
            id={`ocr-documento-${form.itemId}`}
            value={form.documento}
            keyfilter="int"
            onChange={(e) => patch({ documento: e.target.value })}
            className="flit-field-input w-full"
          />
        </FlitFormField>

        <FlitFormField label="Placa" htmlFor={`ocr-placa-${form.itemId}`}>
          <InputText
            id={`ocr-placa-${form.itemId}`}
            value={form.placa}
            onChange={(e) => patch({ placa: e.target.value.toUpperCase() })}
            className="flit-field-input w-full"
          />
        </FlitFormField>

        <FlitFormField label="Infracción" htmlFor={`ocr-infraccion-${form.itemId}`}>
          <InputText
            id={`ocr-infraccion-${form.itemId}`}
            value={form.infraccionCodigo}
            onChange={(e) => patch({ infraccionCodigo: e.target.value })}
            className="flit-field-input w-full"
          />
        </FlitFormField>

        <FlitFormField label="Fecha comparendo" htmlFor={`ocr-fecha-${form.itemId}`}>
          <FlitDateField
            inputId={`ocr-fecha-${form.itemId}`}
            value={form.fechaComparendo}
            onChange={(value) => patch({ fechaComparendo: value })}
          />
        </FlitFormField>

        <FlitFormField label="Total" htmlFor={`ocr-total-${form.itemId}`} error={errors.totalValor}>
          <InputNumber
            inputId={`ocr-total-${form.itemId}`}
            value={parseCurrencyField(form.totalValor)}
            onValueChange={(e) =>
              patch({ totalValor: formatCurrencyField(e.value as number | null) })
            }
            mode="currency"
            currency="COP"
            locale="es-CO"
            minFractionDigits={0}
            maxFractionDigits={0}
            className={`flit-input-number w-full ${errors.totalValor ? "p-invalid" : ""}`}
            inputClassName="flit-field-input w-full"
            aria-invalid={Boolean(errors.totalValor)}
          />
        </FlitFormField>
      </div>

      <div className="mt-4 flex justify-end">
        <Button
          type="button"
          label={confirmed ? "Guardado ✓" : confirming ? "Guardando…" : "Confirmar guardado"}
          className="flit-btn-primary"
          disabled={confirming || confirmed}
          loading={confirming}
          onClick={onConfirm}
          aria-label={`Confirmar comparendo ${form.numeroComparendo || form.fileName}`}
        />
      </div>
    </article>
  );
}
