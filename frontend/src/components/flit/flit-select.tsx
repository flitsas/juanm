"use client";

import { Dropdown, type DropdownProps } from "primereact/dropdown";

export type FlitSelectOption = {
  label: string;
  value: string;
};

type FlitSelectProps = Omit<
  DropdownProps,
  "options" | "onChange" | "optionLabel" | "optionValue" | "panelClassName"
> & {
  options: readonly FlitSelectOption[] | FlitSelectOption[];
  onChange: (value: string) => void;
  invalid?: boolean;
};

export function FlitSelect({
  value,
  options,
  onChange,
  className = "",
  invalid = false,
  appendTo,
  ...rest
}: FlitSelectProps) {
  return (
    <Dropdown
      value={value}
      options={[...options]}
      optionLabel="label"
      optionValue="value"
      onChange={(e) => onChange((e.value as string) ?? "")}
      className={`flit-dropdown w-full ${invalid ? "p-invalid" : ""} ${className}`.trim()}
      panelClassName="flit-dropdown-panel"
      appendTo={appendTo ?? (typeof document !== "undefined" ? document.body : undefined)}
      {...rest}
    />
  );
}
