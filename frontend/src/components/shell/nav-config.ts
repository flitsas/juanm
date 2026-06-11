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
    id: "peticiones",
    label: "Peticiones",
    description: "Derechos de petición y acciones legales.",
    href: "/peticiones",
    icon: Scale,
    enabled: false,
  },
  {
    id: "gdc",
    label: "Plantillas",
    description: "Generador documental jurídico.",
    href: "/gdc",
    icon: FileText,
    enabled: false,
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
    description: "Multi-compañía y configuración NOTIF.",
    href: "/admin/notificaciones",
    icon: Building2,
    enabled: true,
  },
];

export function resolveActiveNavId(pathname: string): string {
  if (pathname.startsWith("/dgc")) return "dgc";
  if (pathname.startsWith("/admin")) return "admin";
  return "dashboard";
}
