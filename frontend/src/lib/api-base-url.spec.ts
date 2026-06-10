import { describe, expect, it } from "vitest";
import { getApiBaseUrl } from "./api-base-url";

describe("getApiBaseUrl", () => {
  it("retorna localhost:4002 por defecto en DEV", () => {
    expect(getApiBaseUrl()).toBe("http://localhost:4002");
  });
});
