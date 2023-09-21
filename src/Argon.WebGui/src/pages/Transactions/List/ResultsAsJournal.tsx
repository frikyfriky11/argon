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
import { Fragment } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import { ITransactionsGetListResponse } from "../../../services/backend/BackendClient";

export type ResultsAsJournalProps = {
  transactions: ITransactionsGetListResponse[];
  page: number;
  rowsPerPage: number;
  totalRows: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
};

export default function ResultsAsJournal({
  transactions,
  page,
  rowsPerPage,
  totalRows,
  onPageChange,
  onPageSizeChange,
  ...other
}: ResultsAsJournalProps & TableContainerProps) {
  const { i18n } = useTranslation();

  return (
    <TableContainer component={Paper} {...other}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Data</TableCell>
            <TableCell>Conto</TableCell>
            <TableCell>Descrizione</TableCell>
            <TableCell align="right">Dare</TableCell>
            <TableCell align="right">Avere</TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {transactions.map((transaction) => (
            <Fragment key={transaction.id}>
              <TableRow>
                <TableCell
                  rowSpan={transaction.transactionRows.length + 2}
                  sx={{ whiteSpace: "nowrap" }}
                >
                  {transaction.date.toISODate()}
                </TableCell>
                <TableCell colSpan={4}>{transaction.description}</TableCell>
                <TableCell
                  align="right"
                  rowSpan={transaction.transactionRows.length + 2}
                >
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
              {transaction.transactionRows.map((row, index) => (
                <TableRow
                  hover
                  key={row.id}
                  sx={{
                    backgroundColor:
                      index % 2
                        ? (theme) => theme.palette.grey.A100
                        : (theme) => theme.palette.grey.A200,
                  }}
                >
                  <TableCell padding="none" sx={{ paddingX: 2 }}>
                    {row.accountName}
                  </TableCell>
                  <TableCell padding="none" sx={{ paddingX: 2 }}>
                    {row.description}
                  </TableCell>
                  <TableCell
                    align="right"
                    padding="none"
                    sx={{
                      fontFamily: "monospace",
                      whiteSpace: "nowrap",
                      paddingX: 2,
                    }}
                  >
                    {row.debit?.toLocaleString(i18n.language, {
                      style: "currency",
                      currency: "EUR",
                    })}
                  </TableCell>
                  <TableCell
                    align="right"
                    padding="none"
                    sx={{
                      fontFamily: "monospace",
                      whiteSpace: "nowrap",
                      paddingX: 2,
                    }}
                  >
                    {row.credit?.toLocaleString(i18n.language, {
                      style: "currency",
                      currency: "EUR",
                    })}
                  </TableCell>
                </TableRow>
              ))}
              <TableRow>
                <TableCell
                  colSpan={6}
                  padding="none"
                  sx={{ paddingY: 1 }}
                ></TableCell>
              </TableRow>
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