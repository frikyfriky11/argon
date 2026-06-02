import { DateTime } from "luxon";

/**
 * Pure helpers that turn the Statistics API responses into the shapes the
 * dashboard charts consume, plus the period-selector maths. Kept free of React
 * and of the generated client so they can be unit-tested in isolation.
 */

export type PeriodPreset = "12m" | "ytd" | "all";

export type Period = {
  from: DateTime | null;
  to: DateTime | null;
};

/**
 * Resolves a preset into a concrete date window. `now` is injected so the
 * computation is deterministic and testable.
 *
 * - "12m": the last 12 full months up to and including the current month.
 * - "ytd": from 1 January of the current year.
 * - "all": no bounds (the whole ledger).
 */
export function resolvePeriod(preset: PeriodPreset, now: DateTime): Period {
  switch (preset) {
    case "12m":
      return {
        from: now.startOf("month").minus({ months: 11 }),
        to: now.endOf("month"),
      };
    case "ytd":
      return {
        from: now.startOf("year"),
        to: now.endOf("month"),
      };
    case "all":
      return { from: null, to: null };
  }
}

/** A short, locale-aware month label for an axis tick, e.g. "gen 2025". */
export function monthLabel(
  year: number,
  month: number,
  locale: string,
): string {
  return DateTime.fromObject({ year, month }).toFormat("LLL yyyy", { locale });
}

/** The most recent balance in a liquidity series, or null when empty. */
export function latestBalance(points: { balance: number }[]): number | null {
  return points.length === 0 ? null : points[points.length - 1].balance;
}

/** Net savings (income − expense) for the most recent cashflow month, or null. */
export function latestNet(
  points: { income: number; expense: number }[],
): number | null {
  if (points.length === 0) {
    return null;
  }
  const last = points[points.length - 1];
  return last.income - last.expense;
}
