"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import {
  resolveActiveNavId,
  SHELL_NAV_ITEMS,
  type ShellNavItem,
} from "./nav-config";
import { FlitIsotipo } from "./FlitLogo";

function DockIconButton({
  item,
  active,
}: {
  item: ShellNavItem;
  active: boolean;
}) {
  const router = useRouter();
  const Icon = item.icon;
  const baseClass =
    "relative z-[1] grid h-11 w-11 place-items-center rounded-full transition-all duration-180";
  const activeClass = active
    ? "bg-[color-mix(in_oklab,var(--action)_18%,white)] text-[var(--action)]"
    : "text-[var(--deep)]/70 hover:bg-[var(--muted)]";

  const tooltip = item.enabled
    ? `${item.label} — ${item.description}`
    : `${item.label} (próximamente)`;

  if (item.enabled) {
    return (
      <Link
        href={item.href}
        aria-current={active ? "page" : undefined}
        aria-label={item.label}
        title={tooltip}
        className={`${baseClass} ${activeClass}`}
        data-testid={`dock-nav-${item.id}`}
        onClick={(event) => {
          if (active) return;
          event.preventDefault();
          router.push(item.href);
        }}
      >
        <Icon className="size-5 transition-colors" aria-hidden />
      </Link>
    );
  }

  return (
    <button
      type="button"
      disabled
      aria-label={`${item.label} — próximamente`}
      title={tooltip}
      className={`${baseClass} cursor-not-allowed opacity-45`}
      data-testid={`dock-nav-${item.id}`}
    >
      <Icon className="size-5" aria-hidden />
    </button>
  );
}

function BrandFab() {
  return (
    <div
      aria-hidden
      className="pointer-events-none my-1 grid h-14 w-14 select-none place-items-center rounded-full shadow-lg ring-2 ring-[var(--lime)]/40"
      style={{ background: "linear-gradient(135deg, #b3ff1f 0%, #003eff 100%)" }}
    >
      <FlitIsotipo size={42} />
    </div>
  );
}

/** Dock vertical derecho — flitready-suite/src/components/right-dock.tsx */
export function RightDock() {
  const pathname = usePathname();
  const activeId = resolveActiveNavId(pathname);
  const topItems = SHELL_NAV_ITEMS.slice(0, 3);
  const bottomItems = SHELL_NAV_ITEMS.slice(3);

  return (
    <>
      <div
        aria-hidden
        className="pointer-events-none fixed top-1/2 right-0 z-[90] -translate-y-1/2 rounded-l-2xl"
        style={{
          width: "30px",
          height: "calc(var(--dock-h, 480px) + 30px)",
          background: "var(--lime)",
        }}
      />
      <nav
        aria-label="Navegación principal"
        className="fixed top-1/2 right-1 z-[100] flex -translate-y-1/2 flex-col items-center gap-1.5 rounded-2xl border border-[var(--border)]/60 bg-[var(--card)]/95 px-2 py-3 shadow-xl backdrop-blur"
        style={{ boxShadow: "0 18px 50px -16px rgba(0,62,255,0.35)" }}
        data-testid="flit-right-dock"
      >
        {topItems.map((item) => (
          <DockIconButton key={item.id} item={item} active={item.id === activeId} />
        ))}
        <BrandFab />
        {bottomItems.map((item) => (
          <DockIconButton key={item.id} item={item} active={item.id === activeId} />
        ))}
      </nav>
    </>
  );
}
