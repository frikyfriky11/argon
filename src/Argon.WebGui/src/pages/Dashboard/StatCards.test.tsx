import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import StatCards from "./StatCards";

describe("StatCards", () => {
  it("shows the latest liquidity balance and the latest monthly net", () => {
    render(
      <StatCards
        cashflow={[
          { year: 2026, month: 4, income: 1000, expense: 600 },
          { year: 2026, month: 5, income: 2000, expense: 1500 },
        ]}
        liquidity={[
          { year: 2026, month: 4, balance: 5000 },
          { year: 2026, month: 5, balance: 5500 },
        ]}
        locale="en"
      />,
    );

    expect(screen.getByText("Patrimonio liquido")).toBeInTheDocument();
    expect(screen.getByText("€5,500.00")).toBeInTheDocument();
    // latest net = 2000 - 1500 = 500
    expect(screen.getByText("€500.00")).toBeInTheDocument();
  });

  it("renders a dash when there is no data", () => {
    render(
      <StatCards cashflow={undefined} liquidity={undefined} locale="en" />,
    );

    expect(screen.getAllByText("—")).toHaveLength(2);
  });
});
