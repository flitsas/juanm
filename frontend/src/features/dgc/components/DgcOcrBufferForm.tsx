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
      className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-4"
      data-testid={`ocr-buffer-item-${form.itemId}`}
    >
      <div className="flex flex-wrap items-start gap-4">
        {form.previewUrl ? (
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
        <Field
          label="No. Comparendo *"
          value={form.numeroComparendo}
          error={errors.numeroComparendo}
          onChange={(v) => patch({ numeroComparendo: v })}
        />
        <Field
          label="Estado *"
          value={form.estado}
          error={errors.estado}
          onChange={(v) => patch({ estado: v })}
        />
        <Field
          label="Infractor"
          value={form.infractorNombre}
          onChange={(v) => patch({ infractorNombre: v })}
        />
        <Field label="Documento" value={form.documento} onChange={(v) => patch({ documento: v })} />
        <Field label="Placa" value={form.placa} onChange={(v) => patch({ placa: v })} />
        <Field
          label="Infracción"
          value={form.infraccionCodigo}
          onChange={(v) => patch({ infraccionCodigo: v })}
        />
        <Field
          label="Fecha comparendo"
          type="date"
          value={form.fechaComparendo}
          onChange={(v) => patch({ fechaComparendo: v })}
        />
        <Field label="Total" value={form.totalValor} onChange={(v) => patch({ totalValor: v })} />
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

function Field({
  label,
  value,
  error,
  onChange,
  type = "text",
}: {
  label: string;
  value: string;
  error?: string;
  onChange: (value: string) => void;
  type?: string;
}) {
  return (
    <label className="block text-sm">
      <span className="mb-1 block text-xs text-[var(--muted-foreground)]">{label}</span>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className={`h-10 w-full rounded-xl border px-3 text-sm outline-none focus:ring-2 focus:ring-[var(--action)] ${
          error ? "border-[var(--alert)]" : "border-[var(--border)]"
        }`}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? `${label}-error` : undefined}
      />
      {error ? (
        <span id={`${label}-error`} className="mt-1 block text-xs text-[var(--alert)]" role="alert">
          {error}
        </span>
      ) : null}
    </label>
  );
}
