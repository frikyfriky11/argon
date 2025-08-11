import { Stack } from "@mui/material";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { DateTime } from "luxon";

import { TransactionsClient } from "../../../services/backend/BackendClient";
import useSearchParamsState from "../../../utils/UrlUtils";
import Filters from "./Filters";
import ResultsAsJournal from "./ResultsAsJournal";
import ResultsAsTable from "./ResultsAsTable";
import Toolbar from "./Toolbar";

export default function Index() {
  const [filters, setFilters] = useSearchParamsState({
    page: 0,
    pageSize: 10,
    accountIds: [] as string[],
    counterpartyIds: [] as string[],
    dateFrom: null as DateTime | null,
    dateTo: null as DateTime | null,
    view: "table" as "table" | "journal",
  });

  const clearFilters = () => {
    setFilters((prev) => ({
      ...prev,
      page: 0,
      accountIds: [],
      counterpartyIds: [],
      dateFrom: null,
      dateTo: null,
    }));
  };

  const transactions = useQuery({
    queryKey: [
      "transactions",
      filters.accountIds,
      filters.counterpartyIds,
      filters.dateFrom,
      filters.dateTo,
      filters.page,
      filters.pageSize,
    ],
    queryFn: () =>
      new TransactionsClient().getList(
        filters.accountIds,
        filters.counterpartyIds,
        filters.dateFrom,
        filters.dateTo,
        filters.page + 1,
        filters.pageSize,
      ),
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
        onSelectedViewChange={(view) => {
          setFilters((prev) => ({ ...prev, view }));
        }}
        selectedView={filters.view}
      />
      <Filters
        accountIds={filters.accountIds}
        dateFrom={filters.dateFrom}
        dateTo={filters.dateTo}
        counterpartyIds={filters.counterpartyIds}
        onAccountIdsChange={(accountIds) => {
          setFilters((prev) => ({ ...prev, accountIds }));
        }}
        onDateFromChange={(dateFrom) => {
          setFilters((prev) => ({ ...prev, dateFrom }));
        }}
        onDateToChange={(dateTo) => {
          setFilters((prev) => ({ ...prev, dateTo }));
        }}
        onCounterpartyIdsChange={(counterpartyIds) => {
          setFilters((prev) => ({ ...prev, counterpartyIds }));
        }}
        onClearFilters={clearFilters}
      />
      {filters.view === "table" && (
        <ResultsAsTable
          onPageChange={(page) => {
            setFilters((prev) => ({ ...prev, page }));
          }}
          onPageSizeChange={(pageSize) => {
            setFilters((prev) => ({ ...prev, pageSize }));
          }}
          page={filters.page}
          rowsPerPage={filters.pageSize}
          totalRows={transactions.data.totalCount}
          transactions={transactions.data.items}
        />
      )}
      {filters.view === "journal" && (
        <ResultsAsJournal
          onPageChange={(page) => {
            setFilters((prev) => ({ ...prev, page }));
          }}
          onPageSizeChange={(pageSize) => {
            setFilters((prev) => ({ ...prev, pageSize }));
          }}
          page={filters.page}
          rowsPerPage={filters.pageSize}
          totalRows={transactions.data.totalCount}
          transactions={transactions.data.items}
        />
      )}
    </Stack>
  );
}
