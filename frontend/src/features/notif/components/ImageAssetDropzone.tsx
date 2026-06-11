"use client";

import { useState } from "react";
import {
  isAcceptedTemplateImage,
  TEMPLATE_IMAGE_ACCEPT,
} from "../lib/accepted-template-image-types";
import { resolveNotifAssetUrl } from "../lib/template-preview";

type ImageAssetDropzoneProps = {
  label: string;
  assetUrl: string;
  uploading?: boolean;
  onUpload: (file: File) => void;
  onClear?: () => void;
  testId: string;
};

export function ImageAssetDropzone({
  label,
  assetUrl,
  uploading,
  onUpload,
  onClear,
  testId,
}: ImageAssetDropzoneProps) {
  const [dragOver, setDragOver] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const processFile = (file: File) => {
    if (!isAcceptedTemplateImage(file)) {
      setError("Solo se permiten imágenes PNG o JPG.");
      return;
    }
    setError(null);
    onUpload(file);
  };

  const resolvedUrl = resolveNotifAssetUrl(assetUrl);

  return (
    <div className="flex flex-col gap-2" data-testid={testId}>
      <span className="text-xs font-medium text-[var(--muted-foreground)]">{label}</span>
      {/* biome-ignore lint/a11y/noStaticElementInteractions: dropzone para input file */}
      <div
        className={`rounded-2xl border-2 border-dashed p-4 text-center transition ${
          dragOver
            ? "border-[var(--action)] bg-[var(--action)]/5"
            : "border-[var(--border)] bg-[var(--muted)]/30"
        }`}
        onDragOver={(e) => {
          e.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(e) => {
          e.preventDefault();
          setDragOver(false);
          const file = e.dataTransfer.files[0];
          if (file) processFile(file);
        }}
      >
        {resolvedUrl ? (
          <div className="space-y-2">
            {/* biome-ignore lint/performance/noImgElement: preview de asset subido */}
            <img
              src={resolvedUrl}
              alt={label}
              className="mx-auto max-h-24 rounded-lg object-contain"
            />
            {onClear ? (
              <button
                type="button"
                className="text-xs text-[var(--alert)] hover:underline"
                onClick={onClear}
              >
                Quitar imagen
              </button>
            ) : null}
          </div>
        ) : (
          <>
            <p className="text-xs text-[var(--muted-foreground)]">
              Arrastra PNG o JPG, o selecciona archivo
            </p>
            <label className="mt-2 inline-block cursor-pointer rounded-full bg-[var(--action)] px-4 py-2 text-xs font-medium text-white">
              Seleccionar
              <input
                type="file"
                accept={TEMPLATE_IMAGE_ACCEPT}
                className="sr-only"
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file) processFile(file);
                }}
              />
            </label>
          </>
        )}
        {uploading ? (
          <p className="mt-2 text-xs text-[var(--muted-foreground)]" role="status">
            Subiendo…
          </p>
        ) : null}
      </div>
      {error ? (
        <p className="text-xs text-[var(--alert)]" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  );
}
