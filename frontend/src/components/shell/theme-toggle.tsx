"use client";

import { Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";
import { useEffect, useState } from "react";

type ThemeToggleProps = {
  /** pill = header (flitready-suite app-header); icon = login top-right */
  variant?: "pill" | "icon";
  className?: string;
};

export function ThemeToggle({ variant = "pill", className = "" }: ThemeToggleProps) {
  const { resolvedTheme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => setMounted(true), []);

  if (!mounted) {
    return (
      <button
        type="button"
        aria-label="Cambiar tema"
        className={`opacity-60 ${variant === "pill" ? "h-9 w-16 rounded-full bg-[var(--muted)]" : "grid h-10 w-10 place-items-center rounded-full"} ${className}`}
        disabled
      />
    );
  }

  const isDark = resolvedTheme === "dark";
  const toggle = () => setTheme(isDark ? "light" : "dark");

  if (variant === "icon") {
    return (
      <button
        type="button"
        aria-label={isDark ? "Activar modo claro" : "Activar modo oscuro"}
        onClick={toggle}
        className={`grid h-10 w-10 place-items-center rounded-full transition-colors hover:bg-[var(--muted)] ${className}`}
        data-testid="theme-toggle"
      >
        {isDark ? <Sun className="size-5" aria-hidden /> : <Moon className="size-5" aria-hidden />}
      </button>
    );
  }

  return (
    <button
      type="button"
      aria-label={isDark ? "Cambiar a modo claro" : "Cambiar a modo oscuro"}
      onClick={toggle}
      className={`relative flex h-9 w-16 items-center rounded-full border border-[var(--border)] bg-[var(--muted)] px-1 transition-colors ${className}`}
      data-testid="theme-toggle"
    >
      <span
        className="grid h-7 w-7 place-items-center rounded-full bg-[var(--card)] shadow transition-transform"
        style={{ transform: isDark ? "translateX(28px)" : "translateX(0)" }}
      >
        {isDark ? <Moon className="size-4" aria-hidden /> : <Sun className="size-4" aria-hidden />}
      </span>
    </button>
  );
}
