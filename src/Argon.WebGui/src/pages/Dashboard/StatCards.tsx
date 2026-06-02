import { Card, CardContent, Grid, Typography } from "@mui/material";
import React from "react";

import {
  IStatisticsCashflowResponse,
  IStatisticsLiquidityResponse,
} from "../../services/backend/BackendClient";
import { formatCurrency } from "../../utils/format";
import { latestBalance, latestNet } from "../../utils/statistics";

export type StatCardsProps = {
  liquidity: IStatisticsLiquidityResponse[] | undefined;
  netWorth: number | undefined;
  cashflow: IStatisticsCashflowResponse[] | undefined;
  locale: string;
};

type Stat = {
  label: string;
  value: number | null;
  color?: "error.main" | "success.main";
};

export default function StatCards({
  liquidity,
  netWorth,
  cashflow,
  locale,
}: StatCardsProps) {
  const balance = liquidity ? latestBalance(liquidity) : null;
  const net = cashflow ? latestNet(cashflow) : null;

  const stats: Stat[] = [
    { label: "Patrimonio liquido", value: balance },
    { label: "Patrimonio netto", value: netWorth ?? null },
    {
      label: "Saldo ultimo mese",
      value: net,
      color: net == null ? undefined : net < 0 ? "error.main" : "success.main",
    },
  ];

  return (
    <Grid container spacing={2}>
      {stats.map((stat) => (
        <Grid item key={stat.label} sm={4} xs={12}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" variant="overline">
                {stat.label}
              </Typography>
              <Typography color={stat.color} variant="h4">
                {stat.value == null ? "—" : formatCurrency(stat.value, locale)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
}
