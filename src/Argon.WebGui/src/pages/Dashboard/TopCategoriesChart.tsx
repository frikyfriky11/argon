import { BarChart } from "@mui/x-charts/BarChart";
import React from "react";

import { IStatisticsTopCategoriesResponse } from "../../services/backend/BackendClient";
import { formatCompactCurrency, formatCurrency } from "../../utils/format";

export type TopCategoriesChartProps = {
  categories: IStatisticsTopCategoriesResponse[];
  locale: string;
  height?: number;
};

export default function TopCategoriesChart({
  categories,
  locale,
  height = 320,
}: TopCategoriesChartProps) {
  // Largest first in the data; reverse so the biggest bar sits at the top of the
  // horizontal axis (which renders bottom-to-top).
  const ordered = [...categories].reverse();

  return (
    <BarChart
      grid={{ vertical: true }}
      height={height}
      layout="horizontal"
      margin={{ left: 160 }}
      series={[
        {
          data: ordered.map((category) => category.total),
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
          data: ordered.map((category) => category.accountName),
          scaleType: "band",
        },
      ]}
    />
  );
}
