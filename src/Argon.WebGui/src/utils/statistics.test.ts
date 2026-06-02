import { DateTime } from "luxon";
import { describe, expect, it } from "vitest";

import {
  latestBalance,
  latestNet,
  monthLabel,
  resolvePeriod,
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
