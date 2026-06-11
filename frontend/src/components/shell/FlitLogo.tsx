import Image from "next/image";
import Link from "next/link";
import { FLIT_BRAND } from "@/lib/flit-brand";

type FlitLogoProps = {
  className?: string;
  showTagline?: boolean;
};

/**
 * Logo blanco/lime — flitready-suite flit-logo.tsx (panel gradiente login).
 */
export function FlitLogo({ className, showTagline = false }: FlitLogoProps) {
  return (
    <div className={className}>
      <Image
        src={FLIT_BRAND.logo}
        alt="Flit Ready"
        width={560}
        height={160}
        className="pointer-events-none h-auto w-full select-none"
        priority
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

type FlitLogoBlueProps = {
  className?: string;
  href?: string;
};

/** Logo azul del header — flitready-suite app-header.tsx */
export function FlitLogoBlue({ className, href = "/" }: FlitLogoBlueProps) {
  const img = (
    <Image
      src={FLIT_BRAND.logoBlue}
      alt="Flit Ready"
      width={180}
      height={56}
      className={`pointer-events-none h-14 w-auto select-none dark:brightness-0 dark:invert ${className ?? ""}`}
      priority
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
    <Image
      src={FLIT_BRAND.isotipo}
      alt=""
      width={size}
      height={size}
      className={`brightness-0 invert ${className ?? ""}`}
      aria-hidden
      draggable={false}
    />
  );
}
