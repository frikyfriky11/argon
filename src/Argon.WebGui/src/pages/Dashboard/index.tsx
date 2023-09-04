import { Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import React from "react";

import { AccountsClient } from "../../services/backend/BackendClient";
import FavouriteAccounts from "./FavouriteAccounts";

export default function Dashboard() {
  const accounts = useQuery({
    queryKey: ["accounts"],
    queryFn: () => new AccountsClient().getList(undefined),
  });

  if (accounts.isLoading) {
    return <p>Loading accounts...</p>;
  }

  if (accounts.isError) {
    return <p>Error while loading accounts...</p>;
  }

  return (
    <Stack spacing={4}>
      <Typography variant="h4">Dashboard</Typography>
      <FavouriteAccounts accounts={accounts.data} />
    </Stack>
  );
}
