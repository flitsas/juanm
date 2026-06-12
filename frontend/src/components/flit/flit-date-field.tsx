"use client";

import { Calendar, type CalendarProps } from "primereact/calendar";
import { formatFilterDate, parseFilterDate } from "@/features/dgc/lib/filter-date";

/** Formato visual estándar FLIT / Lovable (es-CO). */
export const FLIT_DATE_FORMAT = "dd/mm/yy";

type FlitDateFieldProps = Omit<
  CalendarProps,
  "value" | "onChange" | "dateFormat" | "inputClassName" | "panelClassName"
> & {
  value: string;
  onChange: (isoDate: string) => void;
  invalid?: boolean;
};

export function FlitDateField({
  value,
  onChange,
  className = "",
  invalid = false,
  appendTo,
  ...rest
}: FlitDateFieldProps) {
  return (
    <Calendar
      value={parseFilterDate(value)}
      onChange={(e) => onChange(formatFilterDate(e.value as Date | null))}
      dateFormat={FLIT_DATE_FORMAT}
      showIcon
      showButtonBar
      appendTo={appendTo ?? (typeof document !== "undefined" ? document.body : undefined)}
      className={`flit-calendar w-full ${className}`.trim()}
      inputClassName={`flit-field-input flit-field-input--calendar w-full ${invalid ? "p-invalid" : ""}`.trim()}
      panelClassName="flit-datepicker-panel"
      {...rest}
    />
  );
}
