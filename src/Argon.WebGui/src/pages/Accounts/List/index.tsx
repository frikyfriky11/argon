import { Stack } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";

import {
  AccountType,
  AccountsClient,
} from "../../../services/backend/BackendClient";
import Results from "./Results";
import Toolbar from "./Toolbar";

export default function Index() {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [accountType, setAccountType] = useState<AccountType | null>(null);

  const accounts = useQuery({
    queryKey: ["accounts"],
    queryFn: () => new AccountsClient().getList(null, null),
  });

  if (accounts.isLoading) {
    return <p>Loading accounts...</p>;
  }

  if (accounts.isError) {
    return <p>Error while loading accounts...</p>;
  }

  const filteredAccounts = accounts.data.filter((account) =>
    accountType !== null ? account.type === accountType : true,
  );

  return (
    <Stack spacing={4}>
      <Toolbar />
      <Results
        accountType={accountType}
        accounts={filteredAccounts.slice(
          page * rowsPerPage,
          (page + 1) * rowsPerPage,
        )}
        onAccountTypeChange={setAccountType}
        onPageChange={setPage}
        onPageSizeChange={setRowsPerPage}
        page={page}
        rowsPerPage={rowsPerPage}
        totalRows={filteredAccounts.length}
      />
    </Stack>
  );
}
