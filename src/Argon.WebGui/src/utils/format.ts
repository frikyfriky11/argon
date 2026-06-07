/**
 * Locale-aware money formatting helpers used across the dashboard charts.
 * All amounts in Argon are in EUR.
 */

export function formatCurrency(value: number, locale: string): string {
  return value.toLocaleString(locale, {
    style: "currency",
    currency: "EUR",
  });
}

/**
 * A compact currency string for chart axes and dense labels (e.g. "1,2 k €",
 * "3 Mln €" depending on locale). Falls back to the full format below ~1000.
 */
export function formatCompactCurrency(value: number, locale: string): string {
  return value.toLocaleString(locale, {
    style: "currency",
    currency: "EUR",
    notation: "compact",
    maximumFractionDigits: 1,
  });
}

/**
 * Formats a ratio as a locale-aware percentage with at most one fraction digit
 * (e.g. 0.093 → "9,3%"). The input is a fraction, not percentage points.
 */
export function formatPercent(value: number, locale: string): string {
  return value.toLocaleString(locale, {
    style: "percent",
    maximumFractionDigits: 1,
  });
}
