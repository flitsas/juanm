const ACCEPTED_EXTENSIONS = [".png", ".jpg", ".jpeg"] as const;

const ACCEPTED_MIME_TYPES = new Set(["image/png", "image/jpeg"]);

export function isAcceptedTemplateImage(file: File): boolean {
  const lower = file.name.toLowerCase();
  const byExt = ACCEPTED_EXTENSIONS.some((ext) => lower.endsWith(ext));
  const byMime = ACCEPTED_MIME_TYPES.has(file.type);
  return byExt || byMime;
}

export const TEMPLATE_IMAGE_ACCEPT = ACCEPTED_EXTENSIONS.join(",");
