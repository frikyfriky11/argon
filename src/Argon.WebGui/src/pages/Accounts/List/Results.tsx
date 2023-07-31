import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import {
  Button,
  Checkbox,
  Paper,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableContainerProps,
  TableFooter,
  TableHead,
  TablePagination,
  TableRow,
  Tabs,
} from "@mui/material";
import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import AccountTypeConverter from "../../../enums/AccountTypeConverter";
import {
  AccountType,
  IAccountsGetListResponse,
} from "../../../services/backend/BackendClient";

const tabs = [
  { text: "Liquidità", value: AccountType.Cash },
  { text: "Spesa", value: AccountType.Expense },
  { text: "Ricavo", value: AccountType.Revenue },
  { text: "Debito", value: AccountType.Debit },
  { text: "Credito", value: AccountType.Credit },
  { text: "Iniziale", value: AccountType.Setup },
];

export type ResultsProps = {
  accounts: IAccountsGetListResponse[];
  accountType: AccountType | null;
  page: number;
  rowsPerPage: number;
  totalRows: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  onAccountTypeChange: (value: AccountType | null) => void;
};

export default function Results({
  accounts,
  accountType,
  page,
  rowsPerPage,
  totalRows,
  onPageChange,
  onPageSizeChange,
  onAccountTypeChange,
  ...other
}: ResultsProps & TableContainerProps) {
  const { i18n } = useTranslation();

  const [selectedAccountIds, setSelectedAccountIds] = useState<string[]>([]);

  const handleSelectAll = (event: React.ChangeEvent<HTMLInputElement>) => {
    let newSelectedAccountIds: string[];

    if (event.target.checked) {
      newSelectedAccountIds = accounts.map((account) => account.id || "");
    } else {
      newSelectedAccountIds = [];
    }

    setSelectedAccountIds(newSelectedAccountIds);
  };

  const handleSelectOne = (id: string) => {
    const newSelectedAccountIds = selectedAccountIds.includes(id)
      ? selectedAccountIds.filter((accountId) => accountId !== id)
      : [...selectedAccountIds, id];

    setSelectedAccountIds(newSelectedAccountIds);
  };

  return (
    <TableContainer component={Paper} {...other}>
      <Tabs
        onChange={(_, value: AccountType | null) => {
          onAccountTypeChange(value);
          onPageChange(0);
        }}
        value={accountType}
        variant="scrollable"
      >
        <Tab label="Tutti" value={null} />
        {tabs.map((tab) => (
          <Tab key={tab.value} label={tab.text} value={tab.value} />
        ))}
      </Tabs>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell padding="checkbox">
              <Checkbox
                checked={selectedAccountIds.length === accounts.length}
                color="primary"
                indeterminate={
                  selectedAccountIds.length > 0 &&
                  selectedAccountIds.length < accounts.length
                }
                onChange={handleSelectAll}
              />
            </TableCell>
            <TableCell>Nome</TableCell>
            <TableCell>Tipo</TableCell>
            <TableCell align="right">Saldo</TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {accounts.map((account) => (
            <TableRow
              hover
              key={account.id}
              selected={selectedAccountIds.includes(account.id)}
            >
              <TableCell padding="checkbox">
                <Checkbox
                  checked={selectedAccountIds.includes(account.id)}
                  onChange={() => {
                    handleSelectOne(account.id);
                  }}
                  value="true"
                />
              </TableCell>
              <TableCell>{account.name}</TableCell>
              <TableCell>
                {AccountTypeConverter.convert(account.type)}
              </TableCell>
              <TableCell align="right" sx={{ fontFamily: "monospace" }}>
                {account.totalAmount.toLocaleString(i18n.language, {
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
                  to={`/accounts/${account.id}`}
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
