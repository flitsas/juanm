import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useLoginIntroPhase } from "./use-login-intro-phase";

describe("useLoginIntroPhase", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it("avanza a fase 3 cuando intro está deshabilitado", () => {
    const { result } = renderHook(() => useLoginIntroPhase(false));
    expect(result.current).toBe(3);
  });

  it("avanza por las fases 1 → 2 → 3 con intro habilitado", () => {
    vi.spyOn(window, "matchMedia").mockReturnValue({
      matches: false,
      media: "(prefers-reduced-motion: reduce)",
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    } as MediaQueryList);

    const { result } = renderHook(() => useLoginIntroPhase(true));
    expect(result.current).toBe(1);

    act(() => {
      vi.advanceTimersByTime(2800);
    });
    expect(result.current).toBe(2);

    act(() => {
      vi.advanceTimersByTime(200);
    });
    expect(result.current).toBe(3);
  });

  it("salta a fase 3 con prefers-reduced-motion", () => {
    vi.spyOn(window, "matchMedia").mockReturnValue({
      matches: true,
      media: "(prefers-reduced-motion: reduce)",
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    } as MediaQueryList);

    const { result } = renderHook(() => useLoginIntroPhase(true));
    expect(result.current).toBe(3);
  });
});
