import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import {
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableContainerProps,
  TableFooter,
  TableHead,
  TablePagination,
  TableRow,
} from "@mui/material";
import { DateTime } from "luxon";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import {
  ITransactionRowsGetListResponse,
  ITransactionsGetListResponse,
} from "../../../services/backend/BackendClient";

export type ResultsAsTableProps = {
  transactions: ITransactionsGetListResponse[];
  page: number;
  rowsPerPage: number;
  totalRows: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
};

export default function ResultsAsTable({
  transactions,
  page,
  rowsPerPage,
  totalRows,
  onPageChange,
  onPageSizeChange,
  ...other
}: ResultsAsTableProps & TableContainerProps) {
  const { i18n } = useTranslation();

  const stringifyTransactionRows = (
    rows: ITransactionRowsGetListResponse[],
  ) => {
    const fromAccounts = rows
      .filter((row) => row.debit !== null)
      .map((row) => row.accountName);
    const toAccounts = rows
      .filter((row) => row.credit !== null)
      .map((row) => row.accountName);

    if (fromAccounts.length > 0 && toAccounts.length > 0) {
      return `${toAccounts.join(" e ")} -> ${fromAccounts.join(" e ")}`;
    } else {
      return null;
    }
  };

  return (
    <TableContainer component={Paper} {...other}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Data</TableCell>
            <TableCell>Controparte</TableCell>
            <TableCell>Conti</TableCell>
            <TableCell align="right">Importo</TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {transactions.map((transaction) => (
            <TableRow hover key={transaction.id}>
              <TableCell sx={{ whiteSpace: "nowrap" }}>
                {transaction.date
                  .setLocale(i18n.language)
                  .toLocaleString(DateTime.DATE_MED)}
              </TableCell>
              <TableCell>{transaction.counterpartyName}</TableCell>
              <TableCell>
                {stringifyTransactionRows(transaction.transactionRows)}
              </TableCell>
              <TableCell
                align="right"
                sx={{ fontFamily: "monospace", whiteSpace: "nowrap" }}
              >
                {transaction.transactionRows
                  .filter((row) => row.debit !== null)
                  .reduce((acc, row) => acc + (row.debit ?? 0), 0)
                  .toLocaleString(i18n.language, {
                    style: "currency",
                    currency: "EUR",
                  })}
              </TableCell>
              <TableCell align="right">
                <Button
                  color="primary"
                  component={Link}
                  endIcon={<ArrowForwardIcon />}
                  size="small"
                  to={`/transactions/${transaction.id}`}
                  variant="text"
                >
                  Vedi
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
        <TableFooter>
          <TableRow>
            <TablePagination
              count={totalRows}
              onPageChange={(_, page) => {
                onPageChange(page);
              }}
              onRowsPerPageChange={(e) => {
                onPageSizeChange(Number(e.target.value));
              }}
              page={page}
              rowsPerPage={rowsPerPage}
              rowsPerPageOptions={[5, 10, 25]}
            />
          </TableRow>
        </TableFooter>
      </Table>
    </TableContainer>
  );
}
