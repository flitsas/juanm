export const DP_ESTADO_LABELS: Record<string, string> = {
  NoEnviado: "No enviado",
  Enviado: "Enviado",
  SinRespuesta: "Sin respuesta",
  ConRespuesta: "Con respuesta",
};

export const DP_ESTADO_BADGE_CLASS: Record<string, string> = {
  NoEnviado: "border border-[var(--border)] bg-[var(--muted)]/60 text-[var(--deep)]",
  Enviado: "border border-[var(--tech)]/40 bg-[var(--tech)]/15 text-[var(--deep)]",
  SinRespuesta: "border border-[var(--amber-acc)]/40 bg-[var(--amber-acc)]/20 text-[var(--deep)]",
  ConRespuesta: "border border-[var(--action)]/30 bg-[var(--action)]/10 text-[var(--action)]",
};

export function formatDpEstadoLabel(estado: string): string {
  return DP_ESTADO_LABELS[estado] ?? estado;
}
