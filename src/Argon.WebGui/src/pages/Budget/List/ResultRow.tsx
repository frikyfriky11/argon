import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import { Button, TableCell, TableRow } from "@mui/material";
import Decimal from "decimal.js";
import React from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import InputCurrencyMini from "../../../components/InputCurrencyMini";
import {
  AccountType,
  IAccountsGetListResponse,
  IBudgetItemsGetListResponse,
} from "../../../services/backend/BackendClient";

export type ResultRowProps = {
  account: IAccountsGetListResponse;
  accountPreviousMonth?: IAccountsGetListResponse;
  budgetItem?: IBudgetItemsGetListResponse;
  onBudgetItemChange: (accountId: string, value: number | null) => void;
  budgetItemPreviousMonth?: IBudgetItemsGetListResponse;
};

export default function ResultRow({
  account,
  accountPreviousMonth,
  budgetItem,
  onBudgetItemChange,
  budgetItemPreviousMonth,
}: ResultRowProps) {
  const { i18n } = useTranslation();

  const amount =
    account.totalAmount * (account.type === AccountType.Expense ? 1 : -1);
  const amountPreviousMonth =
    (accountPreviousMonth?.totalAmount ?? 0) *
    (account.type === AccountType.Expense ? 1 : -1);
  const delta = new Decimal(amount).minus(budgetItem?.amount ?? 0);
  const deltaPreviousMonth = new Decimal(amountPreviousMonth).minus(
    budgetItemPreviousMonth?.amount ?? 0,
  );
  const isGood =
    (account.type === AccountType.Expense && delta.isNegative()) ||
    (account.type === AccountType.Revenue && delta.isPositive());
  const isGoodPreviousMonth =
    (account.type === AccountType.Expense && deltaPreviousMonth.isNegative()) ||
    (account.type === AccountType.Revenue && deltaPreviousMonth.isPositive());

  return (
    <TableRow hover key={account.id}>
      <TableCell>{account.name}</TableCell>
      <TableCell align="right" sx={{ fontFamily: "monospace", opacity: 0.5 }}>
        {amountPreviousMonth.toLocaleString(i18n.language, {
          style: "currency",
          currency: "EUR",
        })}
      </TableCell>
      <TableCell align="right" sx={{ fontFamily: "monospace", opacity: 0.5 }}>
        {budgetItemPreviousMonth?.amount.toLocaleString(i18n.language, {
          style: "currency",
          currency: "EUR",
        })}
      </TableCell>
      <TableCell
        align="right"
        sx={{
          fontFamily: "monospace",
          color: (theme) => {
            if (deltaPreviousMonth.isZero()) return undefined;
            if (isGoodPreviousMonth) return theme.palette.success.main;
            else return theme.palette.error.main;
          },
          opacity: 0.5,
        }}
      >
        {Number(deltaPreviousMonth.valueOf()).toLocaleString(i18n.language, {
          style: "currency",
          currency: "EUR",
          signDisplay: "exceptZero",
        })}
      </TableCell>
      <TableCell align="right" sx={{ fontFamily: "monospace" }}>
        {amount.toLocaleString(i18n.language, {
          style: "currency",
          currency: "EUR",
        })}
      </TableCell>
      <TableCell align="right" sx={{ fontFamily: "monospace" }}>
        <InputCurrencyMini
          fieldValue={budgetItem?.amount}
          onFieldBlur={(number) => {
            if (budgetItem?.amount != number)
              onBudgetItemChange(account.id, number);
          }}
          size="small"
          sx={{ width: 160 }}
        />
      </TableCell>
      <TableCell
        align="right"
        sx={{
          fontFamily: "monospace",
          color: (theme) => {
            if (delta.isZero()) return undefined;
            if (isGood) return theme.palette.success.main;
            else return theme.palette.error.main;
          },
        }}
      >
        {Number(delta.valueOf()).toLocaleString(i18n.language, {
          style: "currency",
          currency: "EUR",
          signDisplay: "exceptZero",
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
  );
}
