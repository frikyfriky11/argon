import { Stack } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";

import {
  BankStatementsClient,
  BankStatementsGetListRequest,
} from "../../../services/backend/BackendClient";
import Results from "./Results";
import Toolbar from "./Toolbar";

export default function Index() {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const bankStatements = useQuery({
    queryKey: ["bankStatements"],
    queryFn: () =>
      new BankStatementsClient().getList(new BankStatementsGetListRequest()),
  });

  if (bankStatements.isPending) {
    return <p>Loading bank statements...</p>;
  }

  if (bankStatements.isError) {
    return <p>Error while loading bank statements...</p>;
  }

  return (
    <Stack spacing={4}>
      <Toolbar />
      <Results
        bankStatements={bankStatements.data.slice(
          page * rowsPerPage,
          (page + 1) * rowsPerPage,
        )}
        onPageChange={setPage}
        onPageSizeChange={setRowsPerPage}
        page={page}
        rowsPerPage={rowsPerPage}
        totalRows={bankStatements.data.length}
      />
    </Stack>
  );
}
