"use client";

import { useEffect, useState } from "react";

export type LoginIntroPhase = 1 | 2 | 3;

const PHASE_2_MS = 2800;
const PHASE_3_MS = 3000;

function prefersReducedMotion(): boolean {
  if (typeof window === "undefined") return false;
  return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

/**
 * Three-phase login intro: full splash → panel collapse → form reveal.
 * Skips animation when the user prefers reduced motion.
 */
export function useLoginIntroPhase(enabled: boolean): LoginIntroPhase {
  const [phase, setPhase] = useState<LoginIntroPhase>(enabled ? 1 : 3);

  useEffect(() => {
    if (!enabled) {
      setPhase(3);
      return;
    }

    if (prefersReducedMotion()) {
      setPhase(3);
      return;
    }

    const t1 = window.setTimeout(() => setPhase(2), PHASE_2_MS);
    const t2 = window.setTimeout(() => setPhase(3), PHASE_3_MS);

    return () => {
      window.clearTimeout(t1);
      window.clearTimeout(t2);
    };
  }, [enabled]);

  return phase;
}
