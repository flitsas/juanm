const ACCEPTED_EXTENSIONS = [".pdf", ".png", ".jpg", ".jpeg"] as const;

const ACCEPTED_MIME_TYPES = new Set([
  "application/pdf",
  "image/png",
  "image/jpeg",
]);

export function isAcceptedOcrFile(file: File): boolean {
  const lower = file.name.toLowerCase();
  const byExt = ACCEPTED_EXTENSIONS.some((ext) => lower.endsWith(ext));
  const byMime = ACCEPTED_MIME_TYPES.has(file.type);
  return byExt || byMime;
}

export const OCR_ACCEPT_ATTRIBUTE = ACCEPTED_EXTENSIONS.join(",");
