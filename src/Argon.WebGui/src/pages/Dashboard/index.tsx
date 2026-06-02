import { Grid, Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { DateTime } from "luxon";
import React, { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

import {
  AccountsClient,
  StatisticsClient,
} from "../../services/backend/BackendClient";
import { PeriodPreset, resolvePeriod } from "../../utils/statistics";
import CashflowChart from "./CashflowChart";
import ChartCard from "./ChartCard";
import FavouriteAccounts from "./FavouriteAccounts";
import LiquidityChart from "./LiquidityChart";
import PeriodSelector from "./PeriodSelector";
import StatCards from "./StatCards";
import TopCategoriesChart from "./TopCategoriesChart";
import TopCounterpartiesChart from "./TopCounterpartiesChart";

const TOP_N = 8;

export default function Dashboard() {
  const { i18n } = useTranslation();
  const [preset, setPreset] = useState<PeriodPreset>("12m");
  const period = useMemo(() => resolvePeriod(preset, DateTime.now()), [preset]);

  const accounts = useQuery({
    queryKey: ["accounts"],
    queryFn: () => new AccountsClient().getList(null, null),
  });

  // period.from/to are luxon DateTimes; their toJSON() yields a stable ISO
  // string, so they hash deterministically as query-key entries.
  const liquidity = useQuery({
    queryKey: ["statistics", "liquidity", period.from, period.to],
    queryFn: () => new StatisticsClient().liquidity(period.from, period.to),
  });

  // Net worth is a point-in-time snapshot of the whole balance sheet, so it is
  // independent of the selected period.
  const netWorth = useQuery({
    queryKey: ["statistics", "net-worth"],
    queryFn: () => new StatisticsClient().netWorth(undefined),
  });

  const cashflow = useQuery({
    queryKey: ["statistics", "cashflow", period.from, period.to],
    queryFn: () => new StatisticsClient().cashflow(period.from, period.to),
  });

  const topCategories = useQuery({
    queryKey: ["statistics", "top-categories", period.from, period.to],
    queryFn: () =>
      new StatisticsClient().topCategories(period.from, period.to, TOP_N),
  });

  const topCounterparties = useQuery({
    queryKey: ["statistics", "top-counterparties", period.from, period.to],
    queryFn: () =>
      new StatisticsClient().topCounterparties(period.from, period.to, TOP_N),
  });

  const categoriesCoverage =
    topCategories.data && topCategories.data.length > 0
      ? `Le prime ${topCategories.data.length} categorie coprono il ${topCategories.data[
          topCategories.data.length - 1
        ].cumulativePercentage.toFixed(0)}% della spesa del periodo.`
      : undefined;

  return (
    <Stack spacing={4}>
      <Stack
        alignItems="center"
        direction="row"
        flexWrap="wrap"
        gap={2}
        justifyContent="space-between"
      >
        <Typography variant="h4">Dashboard</Typography>
        <PeriodSelector onChange={setPreset} value={preset} />
      </Stack>

      <StatCards
        cashflow={cashflow.data}
        liquidity={liquidity.data}
        locale={i18n.language}
        netWorth={netWorth.data?.total}
      />

      <Grid container spacing={2}>
        <Grid item md={6} xs={12}>
          <ChartCard
            isEmpty={liquidity.data?.length === 0}
            isError={liquidity.isError}
            isPending={liquidity.isPending}
            title="Patrimonio liquido"
          >
            {liquidity.data && (
              <LiquidityChart locale={i18n.language} points={liquidity.data} />
            )}
          </ChartCard>
        </Grid>

        <Grid item md={6} xs={12}>
          <ChartCard
            isEmpty={cashflow.data?.length === 0}
            isError={cashflow.isError}
            isPending={cashflow.isPending}
            title="Entrate vs Uscite"
          >
            {cashflow.data && (
              <CashflowChart locale={i18n.language} points={cashflow.data} />
            )}
          </ChartCard>
        </Grid>

        <Grid item md={6} xs={12}>
          <ChartCard
            isEmpty={topCategories.data?.length === 0}
            isError={topCategories.isError}
            isPending={topCategories.isPending}
            subtitle={categoriesCoverage}
            title="Categorie di spesa principali"
          >
            {topCategories.data && (
              <TopCategoriesChart
                categories={topCategories.data}
                locale={i18n.language}
              />
            )}
          </ChartCard>
        </Grid>

        <Grid item md={6} xs={12}>
          <ChartCard
            isEmpty={topCounterparties.data?.length === 0}
            isError={topCounterparties.isError}
            isPending={topCounterparties.isPending}
            title="Controparti principali"
          >
            {topCounterparties.data && (
              <TopCounterpartiesChart
                counterparties={topCounterparties.data}
                locale={i18n.language}
              />
            )}
          </ChartCard>
        </Grid>
      </Grid>

      {accounts.data && <FavouriteAccounts accounts={accounts.data} />}
    </Stack>
  );
}
