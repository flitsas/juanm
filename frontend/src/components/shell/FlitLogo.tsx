import Link from "next/link";
import { FLIT_BRAND } from "@/lib/flit-brand";

type FlitLogoProps = {
  className?: string;
  showTagline?: boolean;
};

/**
 * Logo blanco/lime — flitready-suite flit-logo.tsx (panel gradiente login).
 * Usa <img> nativo como el prototipo Lovable (wordmark + brackets ya en el asset).
 */
export function FlitLogo({ className, showTagline = false }: FlitLogoProps) {
  return (
    <div className={className}>
      <img
        src={FLIT_BRAND.logo}
        alt="Flit Ready"
        className="pointer-events-none h-auto w-full select-none"
        draggable={false}
      />
      {showTagline ? (
        <div
          className="mt-2 text-center text-sm font-light tracking-wide text-white/90"
          aria-hidden
        >
          {"\n"}
        </div>
      ) : null}
    </div>
  );
}

/** Badge compacto con gradiente — flitready-suite FlitLogoBadge */
export function FlitLogoBadge({ className }: { className?: string }) {
  return (
    <div
      className={`flex items-center gap-2 rounded-2xl bg-gradient-flit px-3 py-2 shadow-md ${className ?? ""}`}
    >
      <img
        src={FLIT_BRAND.logo}
        alt="Flit Ready"
        className="h-7 w-auto select-none"
        draggable={false}
      />
    </div>
  );
}

type FlitLogoBlueProps = {
  className?: string;
  href?: string;
};

/** Logo azul del header — flitready-suite app-header.tsx */
export function FlitLogoBlue({ className, href = "/" }: FlitLogoBlueProps) {
  const img = (
    <img
      src={FLIT_BRAND.logoBlue}
      alt="Flit Ready"
      className={`pointer-events-none h-14 w-auto select-none dark:brightness-0 dark:invert ${className ?? ""}`}
      draggable={false}
    />
  );

  if (href) {
    return (
      <Link href={href} className="inline-flex shrink-0" aria-label="Ir al dashboard">
        {img}
      </Link>
    );
  }

  return img;
}

type FlitIsotipoProps = {
  className?: string;
  size?: number;
};

/** Isotipo blanco sobre FAB del dock — flitready-suite right-dock.tsx */
export function FlitIsotipo({ className, size = 42 }: FlitIsotipoProps) {
  return (
    <img
      src={FLIT_BRAND.isotipo}
      alt=""
      width={size}
      height={size}
      className={`brightness-0 invert ${className ?? ""}`}
      style={{ width: size, height: size }}
      aria-hidden
      draggable={false}
    />
  );
}
