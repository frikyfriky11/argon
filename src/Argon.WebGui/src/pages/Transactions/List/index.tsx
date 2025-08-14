import { Stack } from "@mui/material";
import { keepPreviousData, useInfiniteQuery } from "@tanstack/react-query";
import { DateTime } from "luxon";
import { useState } from "react";

import { TransactionsClient } from "../../../services/backend/BackendClient";
import useSearchParamsState from "../../../utils/UrlUtils";
import FiltersDialog from "./FiltersDialog";
import ResultsAsFeed from "./ResultsAsFeed";
import Toolbar from "./Toolbar";

export default function Index() {
  const [filters, setFilters] = useSearchParamsState({
    pageSize: 10,
    accountIds: [] as string[],
    counterpartyIds: [] as string[],
    dateFrom: null as DateTime | null,
    dateTo: null as DateTime | null,
  });

  const [isFiltersDialogOpen, setIsFiltersDialogOpen] = useState(false);

  const clearFilters = () => {
    setFilters((prev) => ({
      ...prev,
      accountIds: [],
      counterpartyIds: [],
      dateFrom: null,
      dateTo: null,
    }));
  };

  const transactions = useInfiniteQuery({
    queryKey: [
      "transactions",
      filters.accountIds,
      filters.counterpartyIds,
      filters.dateFrom,
      filters.dateTo,
      filters.pageSize,
    ],
    queryFn: ({ pageParam }) =>
      new TransactionsClient().getList(
        filters.accountIds,
        filters.counterpartyIds,
        filters.dateFrom,
        filters.dateTo,
        pageParam,
        filters.pageSize,
      ),
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.hasNextPage ? lastPage.pageNumber + 1 : undefined,
    getPreviousPageParam: (firstPage) =>
      firstPage.hasPreviousPage ? firstPage.pageNumber - 1 : undefined,
    placeholderData: keepPreviousData,
  });

  if (transactions.isPending) {
    return <p>Loading transactions...</p>;
  }

  if (transactions.isError) {
    return <p>Error while loading transactions...</p>;
  }

  return (
    <Stack spacing={4}>
      <Toolbar
        onFiltersClick={() => {
          setIsFiltersDialogOpen(true);
        }}
      />
      <FiltersDialog
        open={isFiltersDialogOpen}
        onClose={() => {
          setIsFiltersDialogOpen(false);
        }}
        initialFilters={filters}
        onApply={(newFilters) => {
          setFilters((prev) => ({ ...prev, ...newFilters }));
        }}
        onClear={clearFilters}
      />
      <ResultsAsFeed
        transactions={transactions.data.pages.flatMap((p) => p.items)}
        fetchNextPage={transactions.fetchNextPage}
        hasNextPage={transactions.hasNextPage}
        isFetchingNextPage={transactions.isFetchingNextPage}
      />
    </Stack>
  );
}
