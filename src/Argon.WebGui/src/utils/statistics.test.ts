import { DateTime } from "luxon";
import { describe, expect, it } from "vitest";

import {
  latestBalance,
  latestNet,
  monthLabel,
  monthlyNet,
  resolvePeriod,
  savingsRate,
} from "./statistics";

const now = DateTime.fromISO("2026-06-02T10:00:00");

describe("resolvePeriod", () => {
  it("returns the last 12 months ending with the current month for '12m'", () => {
    const period = resolvePeriod("12m", now);
    expect(period.from?.toISODate()).toBe("2025-07-01");
    expect(period.to?.toISODate()).toBe("2026-06-30");
  });

  it("returns from 1 January of the current year for 'ytd'", () => {
    const period = resolvePeriod("ytd", now);
    expect(period.from?.toISODate()).toBe("2026-01-01");
    expect(period.to?.toISODate()).toBe("2026-06-30");
  });

  it("returns no bounds for 'all'", () => {
    const period = resolvePeriod("all", now);
    expect(period.from).toBeNull();
    expect(period.to).toBeNull();
  });
});

describe("monthLabel", () => {
  it("formats a short month label in the given locale", () => {
    expect(monthLabel(2025, 1, "it")).toBe("gen 2025");
    expect(monthLabel(2025, 12, "en")).toBe("Dec 2025");
  });
});

describe("latestBalance", () => {
  it("returns the last point's balance", () => {
    expect(latestBalance([{ balance: 10 }, { balance: 25 }])).toBe(25);
  });

  it("returns null for an empty series", () => {
    expect(latestBalance([])).toBeNull();
  });
});

describe("latestNet", () => {
  it("returns income minus expense for the last month", () => {
    expect(
      latestNet([
        { income: 100, expense: 40 },
        { income: 200, expense: 250 },
      ]),
    ).toBe(-50);
  });

  it("returns null for an empty series", () => {
    expect(latestNet([])).toBeNull();
  });
});

describe("monthlyNet", () => {
  it("computes income minus expense per month", () => {
    const result = monthlyNet([
      { year: 2026, month: 1, income: 1000, expense: 600 },
      { year: 2026, month: 2, income: 800, expense: 1000 },
    ]);
    expect(result.map((point) => point.net)).toEqual([400, -200]);
  });

  it("averages whatever months are available before the window fills", () => {
    const result = monthlyNet([
      { year: 2026, month: 1, income: 300, expense: 0 },
      { year: 2026, month: 2, income: 600, expense: 0 },
      { year: 2026, month: 3, income: 900, expense: 0 },
      { year: 2026, month: 4, income: 1200, expense: 0 },
    ]);
    // nets are 300, 600, 900, 1200; trailing 3-month averages:
    expect(result.map((point) => point.rollingAverage)).toEqual([
      300, // [300]
      450, // [300, 600]
      600, // [300, 600, 900]
      900, // [600, 900, 1200]
    ]);
  });

  it("returns an empty array for an empty series", () => {
    expect(monthlyNet([])).toEqual([]);
  });
});

describe("savingsRate", () => {
  it("divides aggregate net by aggregate income across the series", () => {
    // total income 3000, total expense 2700 → saved 300 of 3000 = 10%.
    expect(
      savingsRate([
        { income: 1000, expense: 900 },
        { income: 2000, expense: 1800 },
      ]),
    ).toBeCloseTo(0.1);
  });

  it("is negative when spending outran income", () => {
    expect(savingsRate([{ income: 100, expense: 150 }])).toBeCloseTo(-0.5);
  });

  it("returns null when there is no income to divide by", () => {
    expect(savingsRate([{ income: 0, expense: 200 }])).toBeNull();
    expect(savingsRate([])).toBeNull();
  });
});
