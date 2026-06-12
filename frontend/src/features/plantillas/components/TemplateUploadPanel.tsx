"use client";

import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { useRef, useState } from "react";

type TemplateUploadPanelProps = {
  uploading: boolean;
  error: string | null;
  onUpload: (file: File, name?: string) => void;
  onCancel: () => void;
};

export function TemplateUploadPanel({
  uploading,
  error,
  onUpload,
  onCancel,
}: TemplateUploadPanelProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [name, setName] = useState("");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    setSelectedFile(file ?? null);
  };

  const handleSubmit = () => {
    if (!selectedFile) return;
    onUpload(selectedFile, name.trim() || undefined);
  };

  return (
    <div
      className="space-y-4 rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6"
      data-testid="gdc-template-upload-panel"
    >
      <div>
        <h2 className="text-lg font-semibold text-[var(--deep)]">Cargar plantilla PDF</h2>
        <p className="mt-1 text-sm text-[var(--muted-foreground)]">
          Suba un PDF con campos AcroForm. El sistema extraerá los tags automáticamente.
        </p>
      </div>

      <div className="space-y-2">
        <label className="text-sm font-medium text-[var(--deep)]" htmlFor="gdc-template-name">
          Nombre (opcional)
        </label>
        <InputText
          id="gdc-template-name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="flit-field-input w-full"
          placeholder="DP Vialix"
        />
      </div>

      <div className="space-y-2">
        <label className="text-sm font-medium text-[var(--deep)]" htmlFor="gdc-template-file">
          Archivo PDF
        </label>
        <input
          ref={inputRef}
          id="gdc-template-file"
          type="file"
          accept="application/pdf,.pdf"
          className="block w-full text-sm text-[var(--muted-foreground)]"
          onChange={handleFileChange}
          data-testid="gdc-template-file-input"
        />
        {selectedFile ? (
          <p className="text-sm text-[var(--deep)]" data-testid="gdc-selected-file-name">
            {selectedFile.name}
          </p>
        ) : null}
      </div>

      {uploading ? (
        <div
          className="rounded-xl border border-[var(--border)] bg-[var(--muted)]/30 p-4 text-sm text-[var(--muted-foreground)]"
          role="status"
          data-testid="gdc-upload-loading"
        >
          Extrayendo tags AcroForm del PDF…
        </div>
      ) : null}

      {error ? (
        <div
          className="rounded-xl border border-[var(--alert)]/30 bg-[var(--alert)]/10 p-4 text-sm text-[var(--alert)]"
          role="alert"
        >
          {error}
        </div>
      ) : null}

      <div className="flex justify-end gap-2">
        <Button type="button" label="Cancelar" className="flit-btn-secondary" onClick={onCancel} />
        <Button
          type="button"
          label="Cargar y extraer"
          className="flit-btn-primary"
          disabled={!selectedFile || uploading}
          loading={uploading}
          onClick={handleSubmit}
          data-testid="gdc-upload-submit-btn"
        />
      </div>
    </div>
  );
}
