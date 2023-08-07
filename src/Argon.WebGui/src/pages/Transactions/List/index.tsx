import { Stack } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { DateTime } from "luxon";
import { useState } from "react";

import { TransactionsClient } from "../../../services/backend/BackendClient";
import Filters from "./Filters";
import Results from "./Results";
import Toolbar from "./Toolbar";

export default function Index() {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [accountIds, setAccountIds] = useState<string[]>([]);
  const [description, setDescription] = useState<string | null>(null);
  const [dateFrom, setDateFrom] = useState<DateTime | null>(null);
  const [dateTo, setDateTo] = useState<DateTime | null>(null);

  const transactions = useQuery({
    queryKey: [
      "transactions",
      accountIds,
      description,
      dateFrom,
      dateTo,
      page,
      rowsPerPage,
    ],
    queryFn: () =>
      new TransactionsClient().getList(
        accountIds,
        description,
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
      <Toolbar />
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
      <Results
        onPageChange={setPage}
        onPageSizeChange={setRowsPerPage}
        page={page}
        rowsPerPage={rowsPerPage}
        totalRows={transactions.data.totalCount}
        transactions={transactions.data.items}
      />
    </Stack>
  );
}
