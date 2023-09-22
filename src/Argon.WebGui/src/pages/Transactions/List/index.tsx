import { Stack } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { useDebounce } from "@uidotdev/usehooks";
import { DateTime } from "luxon";
import { useState } from "react";

import { TransactionsClient } from "../../../services/backend/BackendClient";
import useSearchParamsState from "../../../utils/UrlUtils";
import Filters from "./Filters";
import ResultsAsJournal from "./ResultsAsJournal";
import ResultsAsTable from "./ResultsAsTable";
import Toolbar from "./Toolbar";

export default function Index() {
  const [page, setPage] = useSearchParamsState<number>("page", 0);
  const [rowsPerPage, setRowsPerPage] = useSearchParamsState<number>(
    "pageSize",
    10,
  );
  const [accountIds, setAccountIds] = useSearchParamsState<string[]>(
    "accountIds",
    [],
  );
  const [description, setDescription] = useSearchParamsState<string>(
    "description",
    "",
  );
  const debouncedDescription = useDebounce(description, 1000);
  const [dateFrom, setDateFrom] = useState<DateTime | null>(null);
  const [dateTo, setDateTo] = useState<DateTime | null>(null);
  const [selectedView, setSelectedView] = useSearchParamsState<
    "table" | "journal"
  >("view", "table");

  const transactions = useQuery({
    queryKey: [
      "transactions",
      accountIds,
      debouncedDescription,
      dateFrom,
      dateTo,
      page,
      rowsPerPage,
    ],
    queryFn: () =>
      new TransactionsClient().getList(
        accountIds,
        debouncedDescription,
        dateFrom,
        dateTo,
        page + 1,
        rowsPerPage,
      ),
    keepPreviousData: true,
  });

  if (transactions.isLoading) {
    return <p>Loading transactions...</p>;
  }

  if (transactions.isError) {
    return <p>Error while loading transactions...</p>;
  }

  return (
    <Stack spacing={4}>
      <Toolbar
        onSelectedViewChange={setSelectedView}
        selectedView={selectedView}
      />
      <Filters
        accountIds={accountIds}
        dateFrom={dateFrom}
        dateTo={dateTo}
        description={description}
        onAccountIdsChange={setAccountIds}
        onDateFromChange={setDateFrom}
        onDateToChange={setDateTo}
        onDescriptionChange={setDescription}
      />
      {selectedView === "table" && (
        <ResultsAsTable
          onPageChange={setPage}
          onPageSizeChange={setRowsPerPage}
          page={page}
          rowsPerPage={rowsPerPage}
          totalRows={transactions.data.totalCount}
          transactions={transactions.data.items}
        />
      )}
      {selectedView === "journal" && (
        <ResultsAsJournal
          onPageChange={setPage}
          onPageSizeChange={setRowsPerPage}
          page={page}
          rowsPerPage={rowsPerPage}
          totalRows={transactions.data.totalCount}
          transactions={transactions.data.items}
        />
      )}
    </Stack>
  );
}
