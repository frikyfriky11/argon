import { useTheme } from "@mui/material";
import { BarChart } from "@mui/x-charts/BarChart";
import React from "react";

import { IStatisticsTopCounterpartiesResponse } from "../../services/backend/BackendClient";
import { formatCompactCurrency, formatCurrency } from "../../utils/format";

export type TopCounterpartiesChartProps = {
  counterparties: IStatisticsTopCounterpartiesResponse[];
  locale: string;
  height?: number;
};

export default function TopCounterpartiesChart({
  counterparties,
  locale,
  height = 320,
}: TopCounterpartiesChartProps) {
  const theme = useTheme();
  // Largest first in the data; reverse so the biggest bar sits at the top.
  const ordered = [...counterparties].reverse();

  return (
    <BarChart
      grid={{ vertical: true }}
      height={height}
      layout="horizontal"
      margin={{ left: 160 }}
      series={[
        {
          color: theme.palette.primary.main,
          data: ordered.map((counterparty) => counterparty.total),
          label: "Spesa",
          valueFormatter: (value) =>
            value == null ? "" : formatCurrency(value, locale),
        },
      ]}
      xAxis={[
        {
          valueFormatter: (value: number) =>
            formatCompactCurrency(value, locale),
        },
      ]}
      yAxis={[
        {
          data: ordered.map((counterparty) => counterparty.counterpartyName),
          scaleType: "band",
        },
      ]}
    />
  );
}
