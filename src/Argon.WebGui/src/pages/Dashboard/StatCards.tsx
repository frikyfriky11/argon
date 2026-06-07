import { Card, CardContent, Grid, Typography } from "@mui/material";
import React from "react";

import {
  IStatisticsCashflowResponse,
  IStatisticsLiquidityResponse,
} from "../../services/backend/BackendClient";
import { formatCurrency, formatPercent } from "../../utils/format";
import { latestBalance, latestNet, savingsRate } from "../../utils/statistics";

export type StatCardsProps = {
  liquidity: IStatisticsLiquidityResponse[] | undefined;
  netWorth: number | undefined;
  cashflow: IStatisticsCashflowResponse[] | undefined;
  locale: string;
};

type Stat = {
  label: string;
  value: number | null;
  format?: "currency" | "percent";
  color?: "error.main" | "success.main";
};

const signColor = (value: number | null): Stat["color"] =>
  value == null ? undefined : value < 0 ? "error.main" : "success.main";

export default function StatCards({
  liquidity,
  netWorth,
  cashflow,
  locale,
}: StatCardsProps) {
  const balance = liquidity ? latestBalance(liquidity) : null;
  const net = cashflow ? latestNet(cashflow) : null;
  const rate = cashflow ? savingsRate(cashflow) : null;

  const stats: Stat[] = [
    { label: "Patrimonio liquido", value: balance },
    { label: "Patrimonio netto", value: netWorth ?? null },
    {
      label: "Saldo ultimo mese",
      value: net,
      color: signColor(net),
    },
    {
      label: "Tasso di risparmio",
      value: rate,
      format: "percent",
      color: signColor(rate),
    },
  ];

  const formatValue = (stat: Stat): string => {
    if (stat.value == null) {
      return "—";
    }
    return stat.format === "percent"
      ? formatPercent(stat.value, locale)
      : formatCurrency(stat.value, locale);
  };

  return (
    <Grid container spacing={2}>
      {stats.map((stat) => (
        <Grid item key={stat.label} md={3} sm={6} xs={12}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" variant="overline">
                {stat.label}
              </Typography>
              <Typography color={stat.color} variant="h4">
                {formatValue(stat)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
}
