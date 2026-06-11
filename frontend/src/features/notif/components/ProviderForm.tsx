"use client";

import { Button } from "primereact/button";
import { Checkbox } from "primereact/checkbox";
import { Dropdown } from "primereact/dropdown";
import { InputNumber } from "primereact/inputnumber";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { useEffect, useState } from "react";
import { useNotifProviderMutations } from "../api/use-provider";
import type { NotifProvider, ProviderType } from "../lib/notif.types";
import {
  buildProviderCredentials,
  EMPTY_PROVIDER_FORM,
  hasProviderErrors,
  type ProviderFormState,
  validateProviderForm,
} from "../lib/provider-validation";

const PROVIDER_OPTIONS = [
  { label: "API REST propia", value: "api" },
  { label: "SendGrid", value: "sendgrid" },
  { label: "FLIT Mail (SMTP)", value: "flit_mail" },
] satisfies { label: string; value: ProviderType }[];

type ProviderFormProps = {
  provider: NotifProvider | null | undefined;
  isLoading?: boolean;
};

export function ProviderForm({ provider, isLoading }: ProviderFormProps) {
  const { save, test } = useNotifProviderMutations();
  const [form, setForm] = useState<ProviderFormState>(EMPTY_PROVIDER_FORM);
  const [errors, setErrors] = useState<ReturnType<typeof validateProviderForm>>({});
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [testMessage, setTestMessage] = useState<string | null>(null);

  useEffect(() => {
    if (isLoading) return;
    setForm((prev) => ({
      ...EMPTY_PROVIDER_FORM,
      providerType: (provider?.providerType as ProviderType) ?? "",
      fromAddress: provider?.fromAddress ?? "",
      testDestino: prev.testDestino,
    }));
    setErrors({});
    setSaveMessage(null);
  }, [provider, isLoading]);

  const update = (patch: Partial<ProviderFormState>) => {
    setForm((prev) => ({ ...prev, ...patch }));
    setErrors({});
    setSaveMessage(null);
    setTestMessage(null);
  };

  const handleSave = async () => {
    const validation = validateProviderForm(form);
    setErrors(validation);
    if (hasProviderErrors(validation)) return;

    const credentials = buildProviderCredentials(form);
    if (!credentials || !form.providerType) return;

    try {
      await save.mutateAsync({
        providerType: form.providerType,
        fromAddress: form.fromAddress.trim(),
        credentials,
      });
      setSaveMessage("Proveedor guardado y validado correctamente.");
    } catch (error) {
      setSaveMessage(error instanceof Error ? error.message : "No se pudo guardar el proveedor.");
    }
  };

  const handleTest = async () => {
    if (!form.testDestino.trim() || !form.testDestino.includes("@")) {
      setErrors((prev) => ({ ...prev, testDestino: "Ingrese un destino de prueba válido." }));
      return;
    }

    const validation = validateProviderForm(form);
    setErrors(validation);
    if (hasProviderErrors(validation)) return;

    const credentials = buildProviderCredentials(form);
    if (!credentials || !form.providerType) return;

    try {
      const result = await test.mutateAsync({
        destino: form.testDestino.trim(),
        providerType: form.providerType,
        fromAddress: form.fromAddress.trim(),
        credentials,
      });
      setTestMessage(result.message);
    } catch (error) {
      setTestMessage(error instanceof Error ? error.message : "La prueba de conexión falló.");
    }
  };

  if (isLoading) {
    return (
      <div
        className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-12 text-center text-sm text-[var(--muted-foreground)]"
        role="status"
      >
        Cargando configuración del proveedor…
      </div>
    );
  }

  return (
    <div
      className="rounded-2xl border border-[var(--border)] bg-[var(--card)] p-6 shadow-sm"
      data-testid="notif-provider-form"
    >
      <div className="mb-6">
        <h2 className="text-lg font-semibold text-[var(--deep)]">Proveedor de email</h2>
        <p className="mt-1 text-sm text-[var(--muted-foreground)]">
          Configure el canal de salida para notificaciones del tenant actual.
          {provider?.hasCredentials ? " Credenciales almacenadas de forma segura." : ""}
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div className="flex flex-col gap-1 md:col-span-2">
          <label htmlFor="provider-type" className="text-xs text-[var(--muted-foreground)]">
            Tipo de proveedor
          </label>
          <Dropdown
            inputId="provider-type"
            value={form.providerType}
            options={PROVIDER_OPTIONS}
            onChange={(e) => update({ providerType: e.value as ProviderType | "" })}
            placeholder="Seleccione proveedor"
            className="flit-dropdown w-full max-w-md"
            panelClassName="flit-dropdown-panel"
            aria-invalid={Boolean(errors.providerType)}
          />
          {errors.providerType ? (
            <span className="text-xs text-[var(--alert)]" role="alert">
              {errors.providerType}
            </span>
          ) : null}
        </div>

        <div className="flex flex-col gap-1 md:col-span-2">
          <label htmlFor="provider-from" className="text-xs text-[var(--muted-foreground)]">
            Remitente (From)
          </label>
          <InputText
            id="provider-from"
            type="email"
            value={form.fromAddress}
            onChange={(e) => update({ fromAddress: e.target.value })}
            className="flit-field-input w-full max-w-md"
            aria-invalid={Boolean(errors.fromAddress)}
          />
          {errors.fromAddress ? (
            <span className="text-xs text-[var(--alert)]" role="alert">
              {errors.fromAddress}
            </span>
          ) : null}
        </div>

        {form.providerType === "api" ? (
          <>
            <div className="flex flex-col gap-1 md:col-span-2">
              <label htmlFor="api-base-url" className="text-xs text-[var(--muted-foreground)]">
                URL base API
              </label>
              <InputText
                id="api-base-url"
                value={form.apiBaseUrl}
                onChange={(e) => update({ apiBaseUrl: e.target.value })}
                className="flit-field-input w-full"
                aria-invalid={Boolean(errors.apiBaseUrl)}
              />
              {errors.apiBaseUrl ? (
                <span className="text-xs text-[var(--alert)]" role="alert">
                  {errors.apiBaseUrl}
                </span>
              ) : null}
            </div>
            <div className="flex flex-col gap-1 md:col-span-2">
              <label htmlFor="api-key" className="text-xs text-[var(--muted-foreground)]">
                API Key (opcional)
              </label>
              <Password
                id="api-key"
                inputId="api-key"
                value={form.apiKey}
                onChange={(e) => update({ apiKey: e.target.value })}
                toggleMask
                feedback={false}
                className="w-full"
                inputClassName="flit-field-input w-full"
              />
            </div>
          </>
        ) : null}

        {form.providerType === "sendgrid" ? (
          <div className="flex flex-col gap-1 md:col-span-2">
            <label htmlFor="sendgrid-key" className="text-xs text-[var(--muted-foreground)]">
              SendGrid API Key
            </label>
            <Password
              id="sendgrid-key"
              inputId="sendgrid-key"
              value={form.sendgridApiKey}
              onChange={(e) => update({ sendgridApiKey: e.target.value })}
              toggleMask
              feedback={false}
              className="w-full"
              inputClassName={`flit-field-input w-full ${errors.sendgridApiKey ? "p-invalid" : ""}`}
              aria-invalid={Boolean(errors.sendgridApiKey)}
            />
            {errors.sendgridApiKey ? (
              <span className="text-xs text-[var(--alert)]" role="alert">
                {errors.sendgridApiKey}
              </span>
            ) : null}
          </div>
        ) : null}

        {form.providerType === "flit_mail" ? (
          <>
            <div className="flex flex-col gap-1">
              <label htmlFor="smtp-host" className="text-xs text-[var(--muted-foreground)]">
                Host SMTP
              </label>
              <InputText
                id="smtp-host"
                value={form.smtpHost}
                onChange={(e) => update({ smtpHost: e.target.value })}
                className="flit-field-input w-full"
                aria-invalid={Boolean(errors.smtpHost)}
              />
              {errors.smtpHost ? (
                <span className="text-xs text-[var(--alert)]" role="alert">
                  {errors.smtpHost}
                </span>
              ) : null}
            </div>
            <div className="flex flex-col gap-1">
              <label htmlFor="smtp-port" className="text-xs text-[var(--muted-foreground)]">
                Puerto
              </label>
              <InputNumber
                inputId="smtp-port"
                value={form.smtpPort ? Number(form.smtpPort) : null}
                onValueChange={(e) =>
                  update({ smtpPort: e.value === null ? "" : String(e.value) })
                }
                useGrouping={false}
                min={1}
                max={65535}
                className="flit-input-number w-full"
                inputClassName="flit-field-input w-full"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label htmlFor="smtp-user" className="text-xs text-[var(--muted-foreground)]">
                Usuario
              </label>
              <InputText
                id="smtp-user"
                value={form.smtpUsername}
                onChange={(e) => update({ smtpUsername: e.target.value })}
                className="flit-field-input w-full"
                aria-invalid={Boolean(errors.smtpUsername)}
              />
              {errors.smtpUsername ? (
                <span className="text-xs text-[var(--alert)]" role="alert">
                  {errors.smtpUsername}
                </span>
              ) : null}
            </div>
            <div className="flex flex-col gap-1">
              <label htmlFor="smtp-pass" className="text-xs text-[var(--muted-foreground)]">
                Contraseña
              </label>
              <Password
                inputId="smtp-pass"
                value={form.smtpPassword}
                onChange={(e) => update({ smtpPassword: e.target.value })}
                toggleMask
                feedback={false}
                className="w-full"
                inputClassName={`flit-field-input w-full ${errors.smtpPassword ? "p-invalid" : ""}`}
                aria-invalid={Boolean(errors.smtpPassword)}
              />
              {errors.smtpPassword ? (
                <span className="text-xs text-[var(--alert)]" role="alert">
                  {errors.smtpPassword}
                </span>
              ) : null}
            </div>
            <div className="flex items-center gap-2 md:col-span-2">
              <Checkbox
                inputId="smtp-ssl"
                checked={form.smtpUseSsl}
                onChange={(e) => update({ smtpUseSsl: Boolean(e.checked) })}
              />
              <label htmlFor="smtp-ssl" className="text-sm text-[var(--deep)]">
                Usar SSL/TLS
              </label>
            </div>
          </>
        ) : null}

        <div className="flex flex-col gap-1 md:col-span-2">
          <label htmlFor="test-destino" className="text-xs text-[var(--muted-foreground)]">
            Correo de prueba
          </label>
          <InputText
            id="test-destino"
            type="email"
            value={form.testDestino}
            onChange={(e) => update({ testDestino: e.target.value })}
            placeholder="ops@tenant.test"
            className="flit-field-input w-full max-w-md"
            aria-invalid={Boolean(errors.testDestino)}
          />
          {errors.testDestino ? (
            <span className="text-xs text-[var(--alert)]" role="alert">
              {errors.testDestino}
            </span>
          ) : null}
        </div>
      </div>

      <div className="mt-6 flex flex-wrap gap-3">
        <Button
          type="button"
          label="Guardar proveedor"
          className="flit-btn-primary"
          loading={save.isPending}
          onClick={() => void handleSave()}
          data-testid="provider-save-btn"
        />
        <Button
          type="button"
          label="Probar conexión"
          className="flit-btn-secondary"
          loading={test.isPending}
          onClick={() => void handleTest()}
          data-testid="provider-test-btn"
        />
      </div>

      {saveMessage ? (
        <p
          className={`mt-4 text-sm ${save.isSuccess ? "text-[var(--deep)]" : "text-[var(--alert)]"}`}
          role="status"
        >
          {saveMessage}
        </p>
      ) : null}

      {testMessage ? (
        <p className="mt-2 text-sm text-[var(--deep)]" role="status">
          {testMessage}
        </p>
      ) : null}
    </div>
  );
}
