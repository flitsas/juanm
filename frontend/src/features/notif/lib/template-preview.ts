import { getApiBaseUrl } from "@/lib/api-base-url";

export const DEFAULT_TEMPLATE_VARIABLES: Record<string, string> = {
  numero_comparendo: "CMP-12345",
  placa: "ABC123",
  infractor: "Juan Pérez",
  documento: "1234567890",
  dias_restantes: "15",
  total: "$350.000",
  secretaria: "Secretaría de Movilidad",
};

export function resolveNotifAssetUrl(url: string | null | undefined): string | null {
  if (!url?.trim()) return null;
  if (url.startsWith("http://") || url.startsWith("https://")) return url;
  return `${getApiBaseUrl()}${url.startsWith("/") ? url : `/${url}`}`;
}

export function mergeTemplateVariables(
  template: string,
  variables: Record<string, string>,
): string {
  let result = template;
  for (const [key, value] of Object.entries(variables)) {
    result = result.replaceAll(`{{${key}}}`, value);
  }
  return result;
}

export function composeTemplatePreviewHtml(
  htmlBody: string,
  bannerUrl: string | null,
  footerUrl: string | null,
  variables: Record<string, string> = DEFAULT_TEMPLATE_VARIABLES,
): string {
  const mergedBody = mergeTemplateVariables(htmlBody, variables);
  let html = mergedBody;

  const banner = resolveNotifAssetUrl(bannerUrl);
  if (banner) {
    html = `<div class="notif-banner"><img src="${banner}" alt="banner" style="max-width:100%;" /></div>${html}`;
  }

  const footer = resolveNotifAssetUrl(footerUrl);
  if (footer) {
    html = `${html}<div class="notif-footer"><img src="${footer}" alt="footer" style="max-width:100%;" /></div>`;
  }

  return html;
}
