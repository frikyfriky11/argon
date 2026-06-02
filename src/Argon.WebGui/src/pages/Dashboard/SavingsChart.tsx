import { useTheme } from "@mui/material";
import { BarPlot } from "@mui/x-charts/BarChart";
import { ChartsGrid } from "@mui/x-charts/ChartsGrid";
import { ChartsLegend } from "@mui/x-charts/ChartsLegend";
import { ChartsReferenceLine } from "@mui/x-charts/ChartsReferenceLine";
import { ChartsTooltip } from "@mui/x-charts/ChartsTooltip";
import { ChartsXAxis } from "@mui/x-charts/ChartsXAxis";
import { ChartsYAxis } from "@mui/x-charts/ChartsYAxis";
import { LinePlot, MarkPlot } from "@mui/x-charts/LineChart";
import { ResponsiveChartContainer } from "@mui/x-charts/ResponsiveChartContainer";
import React from "react";

import { IStatisticsCashflowResponse } from "../../services/backend/BackendClient";
import { formatCompactCurrency, formatCurrency } from "../../utils/format";
import { monthLabel, monthlyNet } from "../../utils/statistics";

export type SavingsChartProps = {
  points: IStatisticsCashflowResponse[];
  locale: string;
  height?: number;
};

/**
 * Monthly net savings (income − expense) as signed bars — green when the month
 * was in surplus, red when it overspent — overlaid with a 3-month rolling
 * average so the underlying trend survives lumpy one-off months. Reads the same
 * cashflow series as {@link CashflowChart} and derives the figures via
 * {@link monthlyNet}.
 */
export default function SavingsChart({
  points,
  locale,
  height = 300,
}: SavingsChartProps) {
  const theme = useTheme();
  const data = monthlyNet(points);
  const valueFormatter = (value: number | null) =>
    value == null ? "" : formatCurrency(value, locale);

  return (
    <ResponsiveChartContainer
      height={height}
      series={[
        {
          type: "bar",
          id: "surplus",
          stack: "net",
          label: "Risparmio",
          color: theme.palette.success.main,
          data: data.map((point) => (point.net >= 0 ? point.net : null)),
          valueFormatter,
        },
        {
          type: "bar",
          id: "deficit",
          stack: "net",
          label: "Disavanzo",
          color: theme.palette.error.main,
          data: data.map((point) => (point.net < 0 ? point.net : null)),
          valueFormatter,
        },
        {
          type: "line",
          id: "rolling-average",
          label: "Media mobile 3 mesi",
          color: theme.palette.primary.main,
          curve: "monotoneX",
          data: data.map((point) => point.rollingAverage),
          valueFormatter,
        },
      ]}
      xAxis={[
        {
          data: data.map((point) =>
            monthLabel(point.year, point.month, locale),
          ),
          scaleType: "band",
          id: "months",
        },
      ]}
      yAxis={[
        {
          valueFormatter: (value: number) =>
            formatCompactCurrency(value, locale),
        },
      ]}
    >
      <ChartsGrid horizontal />
      <ChartsReferenceLine y={0} />
      <BarPlot />
      <LinePlot />
      <MarkPlot />
      <ChartsXAxis axisId="months" />
      <ChartsYAxis />
      <ChartsLegend />
      <ChartsTooltip />
    </ResponsiveChartContainer>
  );
}
