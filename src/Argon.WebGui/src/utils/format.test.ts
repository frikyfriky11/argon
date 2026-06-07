import { describe, expect, it } from "vitest";

import { formatCompactCurrency, formatCurrency, formatPercent } from "./format";

// Intl inserts a (narrow) no-break space before the € symbol; normalise any
// unicode whitespace to a plain space so the assertions don't depend on the
// exact ICU space codepoint.
const norm = (value: string): string => value.replace(/\s/g, " ");

describe("formatCurrency", () => {
  it("formats a value as EUR in the Italian locale with the symbol last", () => {
    // Italian CLDR only groups from 5 digits (minimumGroupingDigits = 2),
    // so 12.340 is grouped but a 4-digit value is not.
    expect(norm(formatCurrency(12340.5, "it"))).toBe("12.340,50 €");
  });

  it("formats negative values", () => {
    expect(norm(formatCurrency(-42, "it"))).toBe("-42,00 €");
  });

  it("uses the locale's grouping and symbol position", () => {
    expect(norm(formatCurrency(1000, "en"))).toBe("€1,000.00");
  });
});

describe("formatCompactCurrency", () => {
  it("abbreviates thousands with at most one fraction digit", () => {
    expect(norm(formatCompactCurrency(1500, "en"))).toBe("€1.5K");
    expect(norm(formatCompactCurrency(1230, "en"))).toBe("€1.2K");
  });

  it("abbreviates millions", () => {
    expect(norm(formatCompactCurrency(2_000_000, "en"))).toBe("€2M");
  });
});

describe("formatPercent", () => {
  it("renders a fraction as a percentage with the locale separator", () => {
    expect(norm(formatPercent(0.093, "it"))).toBe("9,3%");
    expect(norm(formatPercent(0.1, "en"))).toBe("10%");
  });

  it("formats negative ratios", () => {
    expect(norm(formatPercent(-0.5, "en"))).toBe("-50%");
  });
});
