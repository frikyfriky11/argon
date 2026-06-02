import { BarChart } from "@mui/x-charts/BarChart";
import React from "react";

import { IStatisticsCashflowResponse } from "../../services/backend/BackendClient";
import { formatCompactCurrency, formatCurrency } from "../../utils/format";
import { monthLabel } from "../../utils/statistics";

export type CashflowChartProps = {
  points: IStatisticsCashflowResponse[];
  locale: string;
  height?: number;
};

export default function CashflowChart({
  points,
  locale,
  height = 300,
}: CashflowChartProps) {
  const valueFormatter = (value: number | null) =>
    value == null ? "" : formatCurrency(value, locale);

  return (
    <BarChart
      grid={{ horizontal: true }}
      height={height}
      series={[
        {
          data: points.map((point) => point.income),
          label: "Entrate",
          valueFormatter,
        },
        {
          data: points.map((point) => point.expense),
          label: "Uscite",
          valueFormatter,
        },
      ]}
      xAxis={[
        {
          data: points.map((point) =>
            monthLabel(point.year, point.month, locale),
          ),
          scaleType: "band",
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
