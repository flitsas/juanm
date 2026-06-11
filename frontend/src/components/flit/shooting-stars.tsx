type ShootingStarsProps = {
  /** Number of streaks. */
  count?: number;
  /** Tone of the streaks. */
  tone?: "white" | "soft" | "blue";
  /** Animation period (s) — base interval between bursts. */
  period?: number;
  /** Optional explicit color override (CSS color). */
  color?: string;
  className?: string;
};

/** Deterministic pseudo-random values to avoid SSR/client hydration mismatch. */
function streakSeed(index: number, salt: number, min: number, range: number) {
  return min + ((index * 47 + salt * 13) % range);
}

/**
 * Diagonal shooting-star streaks crossing the viewport at -45°.
 * Parent should control z-index; pointer events are disabled.
 */
export function ShootingStars({
  count = 6,
  tone = "white",
  period = 10,
  color,
  className,
}: ShootingStarsProps) {
  return (
    <div
      aria-hidden
      className={`pointer-events-none absolute inset-0 overflow-hidden ${className ?? ""}`}
    >
      {Array.from({ length: count }).map((_, i) => {
        const len = streakSeed(i, 1, 140, 220);
        const dur = 1.6 + streakSeed(i, 2, 0, 14) / 10;
        const delay = (i * period) / count + streakSeed(i, 3, 0, 12) / 10;
        const op = tone === "white" ? 0.85 : tone === "blue" ? 0.7 : 0.35;
        const c =
          color ??
          (tone === "white"
            ? "rgba(255,255,255,0.95)"
            : tone === "blue"
              ? "rgba(0,62,255,0.85)"
              : "rgba(255,255,255,0.55)");

        return (
          <span
            key={i}
            className="absolute top-0 left-0 block rounded-full"
            style={{
              width: `${len}px`,
              height: "1px",
              background: `linear-gradient(90deg, transparent, ${c} 60%, transparent)`,
              boxShadow: `0 0 6px ${c}`,
              transformOrigin: "left center",
              animation: `flit-shoot ${dur}s ${delay}s ease-in infinite`,
              ["--op" as string]: op,
            }}
          />
        );
      })}
    </div>
  );
}
