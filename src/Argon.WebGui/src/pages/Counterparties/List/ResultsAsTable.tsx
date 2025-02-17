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
import { Link } from "react-router-dom";

import { ICounterpartiesGetListResponse } from "../../../services/backend/BackendClient";

export type ResultsAsTableProps = {
  counterparties: ICounterpartiesGetListResponse[];
  page: number;
  rowsPerPage: number;
  totalRows: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
};

export default function ResultsAsTable({
  counterparties,
  page,
  rowsPerPage,
  totalRows,
  onPageChange,
  onPageSizeChange,
  ...other
}: ResultsAsTableProps & TableContainerProps) {
  return (
    <TableContainer component={Paper} {...other}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Nome</TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {counterparties.map((counterparty) => (
            <TableRow hover key={counterparty.id}>
              <TableCell>{counterparty.name}</TableCell>
              <TableCell align="right">
                <Button
                  color="primary"
                  component={Link}
                  endIcon={<ArrowForwardIcon />}
                  size="small"
                  to={`/counterparties/${counterparty.id}`}
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
