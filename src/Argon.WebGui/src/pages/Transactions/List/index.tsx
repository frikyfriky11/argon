import { Stack } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
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

  const transactions = useQuery({
    queryKey: ["transactions", accountIds, description, page, rowsPerPage],
    queryFn: () =>
      new TransactionsClient().getList(
        accountIds,
        description,
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
        description={description}
        onAccountIdsChange={setAccountIds}
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
