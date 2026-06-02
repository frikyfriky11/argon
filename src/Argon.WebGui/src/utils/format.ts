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
