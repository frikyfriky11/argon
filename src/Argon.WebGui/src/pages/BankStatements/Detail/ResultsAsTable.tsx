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
import { amber, blue } from "@mui/material/colors";
import { DateTime } from "luxon";
import { Fragment, ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import {
  ITransactionsGetListResponse,
  TransactionStatus,
} from "../../../services/backend/BackendClient";
import AccountsRenderer from "../../Transactions/List/AccountsRenderer.tsx";
import CounterpartyRenderer from "../../Transactions/List/CounterpartyRenderer.tsx";

export type ResultsAsTableProps = {
  transactions: ITransactionsGetListResponse[];
  page: number;
  rowsPerPage: number;
  totalRows: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  renderActions?: (transaction: ITransactionsGetListResponse) => ReactNode;
  renderRowExpansion?: (transaction: ITransactionsGetListResponse) => ReactNode;
};

export default function ResultsAsTable({
  transactions,
  page,
  rowsPerPage,
  totalRows,
  onPageChange,
  onPageSizeChange,
  renderActions,
  renderRowExpansion,
  ...other
}: ResultsAsTableProps & TableContainerProps) {
  const { i18n } = useTranslation();

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
            <Fragment key={transaction.id}>
              <TableRow
                hover
                sx={{
                  backgroundColor:
                    transaction.status === TransactionStatus.PotentialDuplicate
                      ? amber["50"]
                      : transaction.status ===
                          TransactionStatus.PendingImportReview
                        ? blue["50"]
                        : "",
                }}
              >
                <TableCell sx={{ whiteSpace: "nowrap" }}>
                  {transaction.date
                    .setLocale(i18n.language)
                    .toLocaleString(DateTime.DATE_MED)}
                </TableCell>
                <TableCell>
                  <CounterpartyRenderer
                    effectiveCounterpartyName={transaction.counterpartyName}
                    rawImportData={transaction.rawImportData}
                  />
                </TableCell>
                <TableCell>
                  <AccountsRenderer
                    rows={transaction.transactionRows}
                    rawImportData={transaction.rawImportData}
                  />
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
                  {renderActions ? (
                    renderActions(transaction)
                  ) : (
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
                  )}
                </TableCell>
              </TableRow>
              {renderRowExpansion?.(transaction)}
            </Fragment>
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
