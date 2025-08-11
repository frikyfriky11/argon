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
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import { IBankStatementsGetListResponse } from "../../../services/backend/BackendClient";

export type ResultsProps = {
  bankStatements: IBankStatementsGetListResponse[];
  page: number;
  rowsPerPage: number;
  totalRows: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
};

export default function Results({
  bankStatements,
  page,
  rowsPerPage,
  totalRows,
  onPageChange,
  onPageSizeChange,
  ...other
}: ResultsProps & TableContainerProps) {
  const { i18n } = useTranslation();

  return (
    <TableContainer component={Paper} {...other}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Nome file</TableCell>
            <TableCell>Parser</TableCell>
            <TableCell align="right">Numero transazioni</TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {bankStatements.map((bankStatement) => (
            <TableRow hover key={bankStatement.id}>
              <TableCell>{bankStatement.fileName}</TableCell>
              <TableCell>{bankStatement.parserName}</TableCell>
              <TableCell align="right" sx={{ fontFamily: "monospace" }}>
                {bankStatement.transactionsCount.toLocaleString(i18n.language, {
                  style: "decimal",
                })}
              </TableCell>
              <TableCell align="right">
                <Button
                  color="primary"
                  component={Link}
                  endIcon={<ArrowForwardIcon />}
                  size="small"
                  to={`/bank-statements/${bankStatement.id}`}
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
