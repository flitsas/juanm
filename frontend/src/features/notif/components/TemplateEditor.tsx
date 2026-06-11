"use client";

import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { InputTextarea } from "primereact/inputtextarea";
import { useEffect, useMemo, useState } from "react";
import type { NotifTemplate } from "../lib/notif.types";
import {
  composeTemplatePreviewHtml,
  DEFAULT_TEMPLATE_VARIABLES,
  mergeTemplateVariables,
} from "../lib/template-preview";
import {
  EMPTY_TEMPLATE_FORM,
  hasTemplateErrors,
  type TemplateFormState,
  validateTemplateForm,
} from "../lib/template-validation";
import { useNotifTemplateMutations } from "../api/use-templates";
import { ImageAssetDropzone } from "./ImageAssetDropzone";
import { TemplatePreviewPanel } from "./TemplatePreviewPanel";

type TemplateEditorProps = {
  template: NotifTemplate | null;
  saving?: boolean;
  onCancel: () => void;
  onSaved: () => void;
};

export function TemplateEditor({ template, saving, onCancel, onSaved }: TemplateEditorProps) {
  const { create, update, preview, uploadAsset } = useNotifTemplateMutations();
  const [form, setForm] = useState<TemplateFormState>(EMPTY_TEMPLATE_FORM);
  const [errors, setErrors] = useState<ReturnType<typeof validateTemplateForm>>({});
  const [serverPreview, setServerPreview] = useState<{ subject: string; html: string } | null>(
    null,
  );
  const [uploadTarget, setUploadTarget] = useState<"banner" | "footer" | null>(null);

  useEffect(() => {
    setForm({
      name: template?.name ?? "",
      subject: template?.subject ?? "",
      htmlBody: template?.htmlBody ?? "",
      bannerUrl: template?.bannerUrl ?? "",
      footerUrl: template?.footerUrl ?? "",
    });
    setErrors({});
    setServerPreview(null);
  }, [template]);

  const updateForm = (patch: Partial<TemplateFormState>) => {
    setForm((prev) => ({ ...prev, ...patch }));
    setErrors({});
    setServerPreview(null);
  };

  const livePreview = useMemo(
    () => ({
      subject: mergeTemplateVariables(form.subject, DEFAULT_TEMPLATE_VARIABLES),
      html: composeTemplatePreviewHtml(
        form.htmlBody,
        form.bannerUrl || null,
        form.footerUrl || null,
      ),
    }),
    [form.subject, form.htmlBody, form.bannerUrl, form.footerUrl],
  );

  const previewData = serverPreview ?? livePreview;

  const handleSave = async () => {
    const validation = validateTemplateForm(form);
    setErrors(validation);
    if (hasTemplateErrors(validation)) return;

    const payload = {
      name: form.name.trim(),
      subject: form.subject.trim(),
      htmlBody: form.htmlBody,
      bannerUrl: form.bannerUrl.trim() || null,
      footerUrl: form.footerUrl.trim() || null,
    };

    if (template) {
      await update.mutateAsync({ id: template.id, payload });
    } else {
      await create.mutateAsync(payload);
    }
    onSaved();
  };

  const handleServerPreview = async () => {
    if (!template) return;
    const result = await preview.mutateAsync({ id: template.id });
    setServerPreview({ subject: result.subject, html: result.htmlBody });
  };

  const handleAssetUpload = async (target: "banner" | "footer", file: File) => {
    setUploadTarget(target);
    try {
      const result = await uploadAsset.mutateAsync(file);
      updateForm(target === "banner" ? { bannerUrl: result.url } : { footerUrl: result.url });
    } finally {
      setUploadTarget(null);
    }
  };

  return (
    <div
      className="grid gap-6 lg:grid-cols-2"
      data-testid="notif-template-editor"
    >
      <div className="space-y-4 rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-[var(--deep)]">
          {template ? "Editar plantilla" : "Nueva plantilla"}
        </h2>

        <div className="flex flex-col gap-1">
          <label htmlFor="template-name" className="text-xs text-[var(--muted-foreground)]">
            Nombre
          </label>
          <InputText
            id="template-name"
            value={form.name}
            onChange={(e) => updateForm({ name: e.target.value })}
            className="flit-field-input w-full"
            aria-invalid={Boolean(errors.name)}
          />
          {errors.name ? (
            <span className="text-xs text-[var(--alert)]" role="alert">
              {errors.name}
            </span>
          ) : null}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="template-subject" className="text-xs text-[var(--muted-foreground)]">
            Asunto
          </label>
          <InputText
            id="template-subject"
            value={form.subject}
            onChange={(e) => updateForm({ subject: e.target.value })}
            placeholder="Comparendo {{numero_comparendo}}"
            className="flit-field-input w-full"
            aria-invalid={Boolean(errors.subject)}
          />
          {errors.subject ? (
            <span className="text-xs text-[var(--alert)]" role="alert">
              {errors.subject}
            </span>
          ) : null}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="template-body" className="text-xs text-[var(--muted-foreground)]">
            Cuerpo HTML
          </label>
          <InputTextarea
            id="template-body"
            value={form.htmlBody}
            onChange={(e) => updateForm({ htmlBody: e.target.value })}
            rows={8}
            className="w-full rounded-2xl border border-[var(--border)] p-3 font-mono text-sm"
            aria-invalid={Boolean(errors.htmlBody)}
          />
          {errors.htmlBody ? (
            <span className="text-xs text-[var(--alert)]" role="alert">
              {errors.htmlBody}
            </span>
          ) : null}
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <ImageAssetDropzone
            label="Banner (PNG/JPG)"
            assetUrl={form.bannerUrl}
            uploading={uploadTarget === "banner" && uploadAsset.isPending}
            onUpload={(file) => void handleAssetUpload("banner", file)}
            onClear={() => updateForm({ bannerUrl: "" })}
            testId="template-banner-dropzone"
          />
          <ImageAssetDropzone
            label="Pie (PNG/JPG)"
            assetUrl={form.footerUrl}
            uploading={uploadTarget === "footer" && uploadAsset.isPending}
            onUpload={(file) => void handleAssetUpload("footer", file)}
            onClear={() => updateForm({ footerUrl: "" })}
            testId="template-footer-dropzone"
          />
        </div>

        <div className="flex flex-wrap gap-3 pt-2">
          <Button
            type="button"
            label="Guardar plantilla"
            className="flit-btn-primary"
            loading={saving || create.isPending || update.isPending}
            onClick={() => void handleSave()}
            data-testid="template-save-btn"
          />
          {template ? (
            <Button
              type="button"
              label="Preview API"
              className="flit-btn-secondary"
              loading={preview.isPending}
              onClick={() => void handleServerPreview()}
              data-testid="template-preview-api-btn"
            />
          ) : null}
          <Button
            type="button"
            label="Cancelar"
            className="flit-btn-secondary"
            onClick={onCancel}
          />
        </div>
      </div>

      <TemplatePreviewPanel subject={previewData.subject} html={previewData.html} />
    </div>
  );
}
