import type { ComparendoMaestraItem } from "@/features/dgc/lib/comparendo-maestra.types";

export function hasContraventorIdentificado(comparendo: ComparendoMaestraItem): boolean {
  const nombre = comparendo.contraventorNombre?.trim() || comparendo.contraventor?.trim();
  const documento = comparendo.contraventorDocumento?.trim();
  return Boolean(nombre && documento);
}

export const CONTRAVENTOR_BLOCK_MESSAGE =
  "El comparendo no tiene contraventor identificado. Asocie el contraventor en DGC antes de generar el documento.";
