import type { ComparendoMaestraItem } from "./comparendo-maestra.types";
import type { MaestraColumn } from "./comparendo-maestra-columns";

export function formatMaestraCell(
  item: ComparendoMaestraItem,
  column: MaestraColumn["key"],
): string {
  switch (column) {
    case "fechaComparendo":
    case "fechaNotificacion":
      return item[column] ? formatDate(item[column]) : "—";
    case "total":
      return `$${item.total.toLocaleString("es-CO")}`;
    case "diasRestantes":
      return item.diasRestantes == null ? "—" : String(item.diasRestantes);
    default: {
      const value = item[column];
      return value == null || value === "" ? "—" : String(value);
    }
  }
}

function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split("-");
  if (!year || !month || !day) return isoDate;
  return `${day}/${month}/${year}`;
}
