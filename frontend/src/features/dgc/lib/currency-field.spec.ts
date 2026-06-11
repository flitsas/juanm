import { describe, expect, it } from "vitest";
import { formatCurrencyField, parseCurrencyField } from "./currency-field";

describe("currency-field", () => {
  it("parsea y formatea valores numéricos", () => {
    expect(parseCurrencyField("150000")).toBe(150000);
    expect(formatCurrencyField(150000)).toBe("150000");
    expect(parseCurrencyField("")).toBeNull();
  });
});
