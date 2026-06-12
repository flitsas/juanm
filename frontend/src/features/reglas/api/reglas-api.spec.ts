import { describe, expect, it, vi, beforeEach, afterEach } from "vitest";
import { fetchReglasMatches } from "./reglas-api";

describe("fetchReglasMatches", () => {
  beforeEach(() => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ items: [] }),
      }),
    );
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("incluye runId en query string", async () => {
    await fetchReglasMatches({ runId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" });
    const [url] = vi.mocked(fetch).mock.calls[0] as [string];
    expect(url).toContain("runId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
  });
});
