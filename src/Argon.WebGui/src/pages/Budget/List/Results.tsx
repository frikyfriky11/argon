import {
  Paper,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableContainerProps,
  TableHead,
  TableRow,
  Tabs,
} from "@mui/material";
import { DateTime } from "luxon";
import React from "react";
import { useTranslation } from "react-i18next";

import {
  AccountType,
  IAccountsGetListResponse,
  IBudgetItemsGetListResponse,
} from "../../../services/backend/BackendClient";
import ResultRow from "./ResultRow";

const tabs = [
  { text: "Spesa", value: AccountType.Expense },
  { text: "Ricavo", value: AccountType.Revenue },
];

export type ResultsProps = {
  accounts: IAccountsGetListResponse[];
  accountsPreviousMonth: IAccountsGetListResponse[];
  accountType: AccountType | null;
  budgetItems: IBudgetItemsGetListResponse[];
  budgetItemsPreviousMonth: IBudgetItemsGetListResponse[];
  currentMonthDate: DateTime | null;
  previousMonthDate: DateTime | null;
  onAccountTypeChange: (value: AccountType | null) => void;
  onBudgetItemChange: (accountId: string, value: number | null) => void;
};

export default function Results({
  accounts,
  accountsPreviousMonth,
  accountType,
  budgetItems,
  budgetItemsPreviousMonth,
  currentMonthDate,
  previousMonthDate,
  onAccountTypeChange,
  onBudgetItemChange,
  ...other
}: ResultsProps & TableContainerProps) {
  const { i18n } = useTranslation();

  const getBudgetItem = (
    accountId: string,
  ): IBudgetItemsGetListResponse | undefined =>
    budgetItems.find((budgetItem) => budgetItem.accountId === accountId);

  const getBudgetItemPreviousMonth = (
    accountId: string,
  ): IBudgetItemsGetListResponse | undefined =>
    budgetItemsPreviousMonth.find(
      (budgetItem) => budgetItem.accountId === accountId,
    );

  const getAccountPreviousMonth = (
    accountId: string,
  ): IAccountsGetListResponse | undefined =>
    accountsPreviousMonth.find((account) => account.id === accountId);

  return (
    <TableContainer component={Paper} {...other}>
      <Tabs
        onChange={(_, value: AccountType | null) => {
          onAccountTypeChange(value);
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
            <TableCell>Nome</TableCell>
            <TableCell align="right">
              Effettivo
              <br />
              {previousMonthDate?.setLocale(i18n.language).toLocaleString({
                month: "short",
                year: "numeric",
              })}
            </TableCell>
            <TableCell align="right">
              Budget
              <br />
              {previousMonthDate?.setLocale(i18n.language).toLocaleString({
                month: "short",
                year: "numeric",
              })}
            </TableCell>
            <TableCell align="right">
              Scostamento
              <br />
              {previousMonthDate?.setLocale(i18n.language).toLocaleString({
                month: "short",
                year: "numeric",
              })}
            </TableCell>
            <TableCell align="right">
              Effettivo
              <br />
              {currentMonthDate?.setLocale(i18n.language).toLocaleString({
                month: "short",
                year: "numeric",
              })}
            </TableCell>
            <TableCell align="right">
              Budget
              <br />
              {currentMonthDate?.setLocale(i18n.language).toLocaleString({
                month: "short",
                year: "numeric",
              })}
            </TableCell>
            <TableCell align="right">
              Scostamento
              <br />
              {currentMonthDate?.setLocale(i18n.language).toLocaleString({
                month: "short",
                year: "numeric",
              })}
            </TableCell>
            <TableCell></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {accounts.map((account) => (
            <ResultRow
              account={account}
              accountPreviousMonth={getAccountPreviousMonth(account.id)}
              budgetItem={getBudgetItem(account.id)}
              budgetItemPreviousMonth={getBudgetItemPreviousMonth(account.id)}
              key={account.id}
              onBudgetItemChange={onBudgetItemChange}
            />
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
