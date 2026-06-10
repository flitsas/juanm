import type { ButtonHTMLAttributes, ReactNode } from "react";

type GradientButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  children: ReactNode;
};

export function GradientButton({
  children,
  className = "",
  type = "button",
  ...props
}: GradientButtonProps) {
  return (
    <button
      type={type}
      className={`flit-gradient-btn w-full ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
