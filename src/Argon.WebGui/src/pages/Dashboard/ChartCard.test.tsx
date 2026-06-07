import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import ChartCard from "./ChartCard";

const base = {
  title: "Patrimonio liquido",
  isPending: false,
  isError: false,
  isEmpty: false,
};

describe("ChartCard", () => {
  it("renders its children when there is data", () => {
    render(
      <ChartCard {...base}>
        <div data-testid="chart">chart</div>
      </ChartCard>,
    );

    expect(screen.getByText("Patrimonio liquido")).toBeInTheDocument();
    expect(screen.getByTestId("chart")).toBeInTheDocument();
  });

  it("shows a spinner while pending and hides the children", () => {
    render(
      <ChartCard {...base} isPending>
        <div data-testid="chart">chart</div>
      </ChartCard>,
    );

    expect(screen.getByRole("progressbar")).toBeInTheDocument();
    expect(screen.queryByTestId("chart")).not.toBeInTheDocument();
  });

  it("shows an error message on error", () => {
    render(
      <ChartCard {...base} isError>
        <div data-testid="chart">chart</div>
      </ChartCard>,
    );

    expect(screen.getByText(/Errore/)).toBeInTheDocument();
    expect(screen.queryByTestId("chart")).not.toBeInTheDocument();
  });

  it("shows an empty message when there is no data", () => {
    render(
      <ChartCard {...base} isEmpty>
        <div data-testid="chart">chart</div>
      </ChartCard>,
    );

    expect(screen.getByText(/Nessun dato/)).toBeInTheDocument();
    expect(screen.queryByTestId("chart")).not.toBeInTheDocument();
  });
});
