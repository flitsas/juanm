import type { LucideIcon } from "lucide-react";
import { BarChart3, Building2, FileText, FileWarning, LayoutDashboard, Scale } from "lucide-react";

export type ShellNavItem = {
  id: string;
  label: string;
  description: string;
  href: string;
  icon: LucideIcon;
  enabled: boolean;
};

/** Ítems del RightDock — fuente: flitready-suite/src/components/right-dock.tsx */
export const SHELL_NAV_ITEMS: ShellNavItem[] = [
  {
    id: "dashboard",
    label: "Dashboard",
    description: "Visión general y KPIs en tiempo real.",
    href: "/",
    icon: LayoutDashboard,
    enabled: true,
  },
  {
    id: "dgc",
    label: "Comparendos",
    description: "Gestión integral de comparendos y fotomultas.",
    href: "/dgc",
    icon: FileWarning,
    enabled: true,
  },
  {
    id: "reglas",
    label: "Reglas DP",
    description: "Reglas dinámicas y Derechos de Petición hacia secretarías.",
    href: "/admin/reglas",
    icon: Scale,
    enabled: true,
  },
  {
    id: "gdc",
    label: "Plantillas",
    description: "Generador documental jurídico.",
    href: "/gdc",
    icon: FileText,
    enabled: true,
  },
  {
    id: "reportes",
    label: "Reportes",
    description: "Exportación a Excel y PDF.",
    href: "/reportes",
    icon: BarChart3,
    enabled: false,
  },
  {
    id: "admin",
    label: "Admin",
    description: "Multi-compañía, usuarios, roles y NOTIF.",
    href: "/admin",
    icon: Building2,
    enabled: true,
  },
];

export function resolveActiveNavId(pathname: string): string {
  if (pathname.startsWith("/dgc")) return "dgc";
  if (pathname.startsWith("/gdc")) return "gdc";
  if (pathname.startsWith("/admin/reglas")) return "reglas";
  if (pathname.startsWith("/admin")) return "admin";
  return "dashboard";
}
