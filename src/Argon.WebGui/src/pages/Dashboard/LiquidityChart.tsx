import { useTheme } from "@mui/material";
import { LineChart } from "@mui/x-charts/LineChart";
import React from "react";

import { IStatisticsLiquidityResponse } from "../../services/backend/BackendClient";
import { formatCompactCurrency, formatCurrency } from "../../utils/format";
import { monthLabel } from "../../utils/statistics";

export type LiquidityChartProps = {
  points: IStatisticsLiquidityResponse[];
  locale: string;
  height?: number;
};

export default function LiquidityChart({
  points,
  locale,
  height = 300,
}: LiquidityChartProps) {
  const theme = useTheme();

  return (
    <LineChart
      grid={{ horizontal: true }}
      height={height}
      series={[
        {
          area: true,
          color: theme.palette.primary.main,
          data: points.map((point) => point.balance),
          label: "Saldo",
          showMark: false,
          valueFormatter: (value) =>
            value == null ? "" : formatCurrency(value, locale),
        },
      ]}
      xAxis={[
        {
          data: points.map((point) =>
            monthLabel(point.year, point.month, locale),
          ),
          scaleType: "point",
        },
      ]}
      yAxis={[
        {
          valueFormatter: (value: number) =>
            formatCompactCurrency(value, locale),
        },
      ]}
    />
  );
}
