"use client";

import { Calendar } from "primereact/calendar";
import { Dropdown } from "primereact/dropdown";
import { InputNumber } from "primereact/inputnumber";
import { InputText } from "primereact/inputtext";
import type { OcrBufferFieldErrors, OcrBufferForm } from "../lib/ocr.types";
import { COMPARENDO_ESTADO_OPTIONS } from "../lib/comparendo-estado-options";
import { formatCurrencyField, parseCurrencyField } from "../lib/currency-field";
import { formatFilterDate, parseFilterDate } from "../lib/filter-date";

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
      className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-4"
      data-testid={`ocr-buffer-item-${form.itemId}`}
    >
      <div className="flex flex-wrap items-start gap-4">
        {form.previewUrl ? (
          // biome-ignore lint/performance/noImgElement: blob URL local de vista previa OCR
          <img
            src={form.previewUrl}
            alt={`Vista previa ${form.fileName}`}
            className="h-24 w-24 rounded-xl border border-[var(--border)] object-cover"
          />
        ) : (
          <div className="flex h-24 w-24 items-center justify-center rounded-xl border border-dashed border-[var(--border)] bg-[var(--muted)] text-xs text-[var(--muted-foreground)]">
            {form.fileName.endsWith(".pdf") ? "PDF" : "Archivo"}
          </div>
        )}
        <div className="min-w-[200px] flex-1">
          <p className="text-sm font-semibold text-[var(--deep)]">{form.fileName}</p>
          <p className="text-xs text-[var(--muted-foreground)]">
            Confianza OCR: {(form.confidence * 100).toFixed(0)}%
          </p>
          {form.ocrDiagnostic ? (
            <p className="mt-1 text-xs text-[var(--alert)]" role="note">
              {form.ocrDiagnostic}
            </p>
          ) : null}
        </div>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <TextField
          inputId={`ocr-numero-${form.itemId}`}
          label="No. Comparendo *"
          value={form.numeroComparendo}
          error={errors.numeroComparendo}
          onChange={(value) => patch({ numeroComparendo: value })}
        />
        <SelectField
          inputId={`ocr-estado-${form.itemId}`}
          label="Estado *"
          value={form.estado}
          options={[...COMPARENDO_ESTADO_OPTIONS]}
          error={errors.estado}
          onChange={(value) => patch({ estado: value })}
        />
        <TextField
          inputId={`ocr-infractor-${form.itemId}`}
          label="Infractor"
          value={form.infractorNombre}
          onChange={(value) => patch({ infractorNombre: value })}
        />
        <TextField
          inputId={`ocr-documento-${form.itemId}`}
          label="Documento"
          value={form.documento}
          keyfilter="int"
          onChange={(value) => patch({ documento: value })}
        />
        <TextField
          inputId={`ocr-placa-${form.itemId}`}
          label="Placa"
          value={form.placa}
          onChange={(value) => patch({ placa: value.toUpperCase() })}
        />
        <TextField
          inputId={`ocr-infraccion-${form.itemId}`}
          label="Infracción"
          value={form.infraccionCodigo}
          onChange={(value) => patch({ infraccionCodigo: value })}
        />
        <DateField
          inputId={`ocr-fecha-${form.itemId}`}
          label="Fecha comparendo"
          value={form.fechaComparendo}
          onChange={(value) => patch({ fechaComparendo: value })}
        />
        <CurrencyField
          inputId={`ocr-total-${form.itemId}`}
          label="Total"
          value={form.totalValor}
          error={errors.totalValor}
          onChange={(value) => patch({ totalValor: value })}
        />
      </div>

      <div className="mt-4 flex justify-end">
        <button
          type="button"
          disabled={confirming || confirmed}
          onClick={onConfirm}
          className="rounded-full bg-[var(--action)] px-5 py-2 text-sm font-medium text-white disabled:opacity-50"
          aria-label={`Confirmar comparendo ${form.numeroComparendo || form.fileName}`}
        >
          {confirmed ? "Guardado ✓" : confirming ? "Guardando…" : "Confirmar guardado"}
        </button>
      </div>
    </article>
  );
}

function TextField({
  inputId,
  label,
  value,
  error,
  keyfilter,
  onChange,
}: {
  inputId: string;
  label: string;
  value: string;
  error?: string;
  keyfilter?: "int";
  onChange: (value: string) => void;
}) {
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={inputId} className="text-xs text-[var(--muted-foreground)]">
        {label}
      </label>
      <InputText
        id={inputId}
        value={value}
        keyfilter={keyfilter}
        onChange={(e) => onChange(e.target.value)}
        className={`flit-field-input w-full ${error ? "p-invalid" : ""}`}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${inputId}-error` : undefined}
      />
      {error ? (
        <span id={`${inputId}-error`} className="text-xs text-[var(--alert)]" role="alert">
          {error}
        </span>
      ) : null}
    </div>
  );
}

function SelectField({
  inputId,
  label,
  value,
  options,
  error,
  onChange,
}: {
  inputId: string;
  label: string;
  value: string;
  options: { label: string; value: string }[];
  error?: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={inputId} className="text-xs text-[var(--muted-foreground)]">
        {label}
      </label>
      <Dropdown
        inputId={inputId}
        value={value}
        options={options}
        onChange={(e) => onChange((e.value as string) ?? "")}
        className={`flit-dropdown w-full ${error ? "p-invalid" : ""}`}
        panelClassName="flit-dropdown-panel"
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${inputId}-error` : undefined}
      />
      {error ? (
        <span id={`${inputId}-error`} className="text-xs text-[var(--alert)]" role="alert">
          {error}
        </span>
      ) : null}
    </div>
  );
}

function DateField({
  inputId,
  label,
  value,
  onChange,
}: {
  inputId: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={inputId} className="text-xs text-[var(--muted-foreground)]">
        {label}
      </label>
      <Calendar
        inputId={inputId}
        value={parseFilterDate(value)}
        onChange={(e) => onChange(formatFilterDate(e.value as Date | null))}
        dateFormat="dd/mm/yy"
        showIcon
        showButtonBar
        appendTo={typeof document !== "undefined" ? document.body : undefined}
        className="flit-calendar w-full"
        inputClassName="flit-field-input flit-field-input--calendar w-full"
        panelClassName="flit-datepicker-panel"
      />
    </div>
  );
}

function CurrencyField({
  inputId,
  label,
  value,
  error,
  onChange,
}: {
  inputId: string;
  label: string;
  value: string;
  error?: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={inputId} className="text-xs text-[var(--muted-foreground)]">
        {label}
      </label>
      <InputNumber
        inputId={inputId}
        value={parseCurrencyField(value)}
        onValueChange={(e) => onChange(formatCurrencyField(e.value as number | null))}
        mode="currency"
        currency="COP"
        locale="es-CO"
        minFractionDigits={0}
        maxFractionDigits={0}
        className={`flit-input-number w-full ${error ? "p-invalid" : ""}`}
        inputClassName="flit-field-input w-full"
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${inputId}-error` : undefined}
      />
      {error ? (
        <span id={`${inputId}-error`} className="text-xs text-[var(--alert)]" role="alert">
          {error}
        </span>
      ) : null}
    </div>
  );
}
