import { Stack } from "@mui/material";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { DateTime } from "luxon";
import { useState } from "react";

import {
  AccountType,
  AccountsClient,
  BudgetItemsClient,
  BudgetItemsUpsertRequest,
  IBudgetItemsUpsertRequest,
} from "../../../services/backend/BackendClient";
import Filters from "./Filters";
import Results from "./Results";
import Toolbar from "./Toolbar";

export default function Index() {
  const queryClient = useQueryClient();

  const [date, setDate] = useState<DateTime | null>(DateTime.now());
  const [accountType, setAccountType] = useState<AccountType | null>(null);

  const year = date?.year;
  const month = date?.month;
  const startOfMonth = DateTime.fromObject({ year, month });
  const endOfMonth = DateTime.fromObject({ year, month }).endOf("month");
  const startOfPreviousMonth = startOfMonth.minus({ month: 1 });
  const endOfPreviousMonth = endOfMonth.minus({ month: 1 });

  const accounts = useQuery({
    queryKey: ["accounts", startOfMonth, endOfMonth],
    queryFn: () => new AccountsClient().getList(startOfMonth, endOfMonth),
    placeholderData: keepPreviousData,
  });

  const accountsPreviousMonth = useQuery({
    queryKey: ["accounts", startOfPreviousMonth, endOfPreviousMonth],
    queryFn: () =>
      new AccountsClient().getList(startOfPreviousMonth, endOfPreviousMonth),
    placeholderData: keepPreviousData,
  });

  const budgetItems = useQuery({
    queryKey: ["budgetItems", year, month],
    queryFn: () => new BudgetItemsClient().getList(year, month),
    placeholderData: keepPreviousData,
  });

  const budgetItemsPreviousMonth = useQuery({
    queryKey: [
      "budgetItems",
      startOfPreviousMonth.year,
      startOfPreviousMonth.month,
    ],
    queryFn: () =>
      new BudgetItemsClient().getList(
        startOfPreviousMonth.year,
        startOfPreviousMonth.month,
      ),
    placeholderData: keepPreviousData,
  });

  const mutation = useMutation({
    mutationFn: async (data: IBudgetItemsUpsertRequest) =>
      new BudgetItemsClient().upsert(new BudgetItemsUpsertRequest(data)),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["accounts", year, month],
      });
      await queryClient.invalidateQueries({
        queryKey: ["budgetItems", year, month],
      });
    },
  });

  if (accounts.isPending) {
    return <p>Loading accounts...</p>;
  }

  if (accountsPreviousMonth.isPending) {
    return <p>Loading accounts previous month...</p>;
  }

  if (budgetItems.isPending) {
    return <p>Loading budget items...</p>;
  }

  if (budgetItemsPreviousMonth.isPending) {
    return <p>Loading budget items previous month...</p>;
  }

  if (accounts.isError) {
    return <p>Error while loading accounts...</p>;
  }

  if (accountsPreviousMonth.isError) {
    return <p>Error while loading accounts previous month...</p>;
  }

  if (budgetItems.isError) {
    return <p>Error while loading budget items...</p>;
  }

  if (budgetItemsPreviousMonth.isError) {
    return <p>Error while loading budget items previous month...</p>;
  }

  const filteredAccounts = accounts.data.filter((account) =>
    accountType !== null
      ? account.type === accountType
      : [AccountType.Expense, AccountType.Revenue].includes(account.type),
  );

  const filteredAccountsPreviousMonth = accountsPreviousMonth.data.filter(
    (account) =>
      accountType !== null
        ? account.type === accountType
        : [AccountType.Expense, AccountType.Revenue].includes(account.type),
  );

  const filteredBudgetItems = budgetItems.data.filter((budgetItem) =>
    filteredAccounts
      .map((account) => account.id)
      .includes(budgetItem.accountId),
  );

  const filteredBudgetItemsPreviousMonth = budgetItemsPreviousMonth.data.filter(
    (budgetItem) =>
      filteredAccounts
        .map((account) => account.id)
        .includes(budgetItem.accountId),
  );

  return (
    <Stack spacing={4}>
      <Toolbar />
      <Filters date={date} onDateChange={setDate} />
      <Results
        accountType={accountType}
        accounts={filteredAccounts}
        accountsPreviousMonth={filteredAccountsPreviousMonth}
        budgetItems={filteredBudgetItems}
        budgetItemsPreviousMonth={filteredBudgetItemsPreviousMonth}
        currentMonthDate={startOfMonth}
        onAccountTypeChange={setAccountType}
        onBudgetItemChange={(accountId, amount) => {
          if (year && month) {
            mutation.mutate({
              year,
              month,
              accountId,
              amount,
            });
          }
        }}
        previousMonthDate={startOfPreviousMonth}
      />
    </Stack>
  );
}
