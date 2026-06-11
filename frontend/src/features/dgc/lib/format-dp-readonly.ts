export type DpReadonlyDisplay = {
  label: string;
  href: string | null;
  isLink: boolean;
};

const URL_PATTERN = /^https?:\/\//i;

export function formatDpReadonly(dp: string | null | undefined): DpReadonlyDisplay {
  if (!dp || !dp.trim()) {
    return { label: "Sin trámite DP asociado", href: null, isLink: false };
  }

  const value = dp.trim();
  if (URL_PATTERN.test(value)) {
    return { label: value, href: value, isLink: true };
  }

  return { label: value, href: null, isLink: false };
}
