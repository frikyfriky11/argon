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

/** Number of trailing months averaged by the savings rolling-average line. */
const ROLLING_WINDOW = 3;

export type SavingsPoint = {
  year: number;
  month: number;
  /** Income − expense for the month; negative when spending outran income. */
  net: number;
  /**
   * Trailing average of `net` over the last {@link ROLLING_WINDOW} months
   * (fewer near the start of the series), to smooth out lumpy one-off months.
   */
  rollingAverage: number;
};

/**
 * Turns a cashflow series into per-month net savings plus a trailing
 * rolling average. The rolling average uses whatever months are available
 * when fewer than {@link ROLLING_WINDOW} precede a point, so the line spans
 * the whole window rather than starting blank.
 */
export function monthlyNet(
  points: { year: number; month: number; income: number; expense: number }[],
): SavingsPoint[] {
  const nets = points.map((point) => point.income - point.expense);
  return points.map((point, index) => {
    const start = Math.max(0, index - (ROLLING_WINDOW - 1));
    const window = nets.slice(start, index + 1);
    const rollingAverage =
      window.reduce((sum, value) => sum + value, 0) / window.length;
    return {
      year: point.year,
      month: point.month,
      net: nets[index],
      rollingAverage,
    };
  });
}

/**
 * The savings rate over the whole series: (income − expense) / income.
 * Aggregating across months rather than averaging monthly rates keeps a few
 * big-income months from dominating. Returns null when there is no income to
 * divide by (an empty period, or one with only expenses).
 */
export function savingsRate(
  points: { income: number; expense: number }[],
): number | null {
  const totals = points.reduce(
    (acc, point) => ({
      income: acc.income + point.income,
      expense: acc.expense + point.expense,
    }),
    { income: 0, expense: 0 },
  );
  if (totals.income <= 0) {
    return null;
  }
  return (totals.income - totals.expense) / totals.income;
}
