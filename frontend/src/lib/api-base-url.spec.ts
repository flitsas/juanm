import { describe, expect, it } from "vitest";
import { getApiBaseUrl } from "./api-base-url";

describe("getApiBaseUrl", () => {
  it("retorna localhost:40303 (core-api directo) por defecto en DEV", () => {
    expect(getApiBaseUrl()).toBe("http://localhost:40303");
  });
});
