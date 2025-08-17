import {
  Button,
  Collapse,
  Paper,
  TableCell,
  TableContainer,
  TableContainerProps,
  TableRow,
} from "@mui/material";
import { useState } from "react";
import { Link } from "react-router-dom";

import {
  ITransactionsGetListResponse,
  TransactionStatus,
} from "../../../services/backend/BackendClient.ts";
import DuplicateResolver from "./DuplicateResolver.tsx";
import ResultsAsTable from "./ResultsAsTable.tsx";

export type ResultsProps = {
  transactions: ITransactionsGetListResponse[];
} & TableContainerProps;

export default function Results({
  transactions,
  ...other
}: ResultsProps & TableContainerProps) {
  const [resolvingId, setResolvingId] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  return (
    <TableContainer component={Paper} {...other}>
      <ResultsAsTable
        onPageChange={setPage}
        onPageSizeChange={(size) => {
          setRowsPerPage(size);
          setPage(0);
        }}
        page={page}
        rowsPerPage={rowsPerPage}
        totalRows={transactions.length}
        transactions={transactions.slice(
          page * rowsPerPage,
          page * rowsPerPage + rowsPerPage,
        )}
        renderActions={(transaction) => {
          switch (transaction.status) {
            case TransactionStatus.PendingImportReview:
              return (
                <Button
                  component={Link}
                  size="small"
                  to={`/transactions/${transaction.id}`}
                  variant="outlined"
                >
                  Completa
                </Button>
              );
            case TransactionStatus.PotentialDuplicate:
              return (
                <Button
                  onClick={() => {
                    setResolvingId(
                      resolvingId === transaction.id ? null : transaction.id,
                    );
                  }}
                  size="small"
                  variant="outlined"
                >
                  Risolvi
                </Button>
              );
            default:
              return null;
          }
        }}
        renderRowExpansion={(transaction) => {
          if (transaction.id !== resolvingId) {
            return null;
          }

          return (
            <TableRow>
              <TableCell colSpan={5} sx={{ p: 0 }}>
                <Collapse
                  in={resolvingId === transaction.id}
                  timeout="auto"
                  unmountOnExit
                >
                  <DuplicateResolver transaction={transaction} />
                </Collapse>
              </TableCell>
            </TableRow>
          );
        }}
      />
    </TableContainer>
  );
}
