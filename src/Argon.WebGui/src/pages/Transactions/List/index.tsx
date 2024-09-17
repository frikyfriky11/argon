import { Stack } from "@mui/material";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { useDebounce } from "@uidotdev/usehooks";
import { DateTime } from "luxon";
import { useState } from "react";

import { TransactionsClient } from "../../../services/backend/BackendClient";
import Filters from "./Filters";
import ResultsAsJournal from "./ResultsAsJournal";
import ResultsAsTable from "./ResultsAsTable";
import Toolbar from "./Toolbar";

export default function Index() {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [accountIds, setAccountIds] = useState<string[]>([]);
  const [description, setDescription] = useState<string | null>(null);
  const debouncedDescription = useDebounce(description, 1000);
  const [dateFrom, setDateFrom] = useState<DateTime | null>(null);
  const [dateTo, setDateTo] = useState<DateTime | null>(null);
  const [selectedView, setSelectedView] = useState<"table" | "journal">(
    "table",
  );

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
