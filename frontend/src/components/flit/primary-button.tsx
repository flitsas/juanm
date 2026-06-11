import type { ButtonHTMLAttributes, ReactNode } from "react";

type PrimaryButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  children: ReactNode;
  size?: "sm" | "md";
  icon?: ReactNode;
};

export function PrimaryButton({
  children,
  size = "md",
  icon,
  className = "",
  type = "button",
  ...props
}: PrimaryButtonProps) {
  const sizeClass = size === "sm" ? "h-8 px-3 text-xs gap-1.5" : "h-10 px-5 text-sm gap-2";

  return (
    <button
      type={type}
      className={`flit-btn-primary inline-flex items-center justify-center ${sizeClass} ${className}`}
      {...props}
    >
      {icon}
      {children}
    </button>
  );
}
