import { Box } from "@mui/material";
import React from "react";

import AccountsFilter from "./AccountsFilter";

export type FiltersProps = {
  onAccountIdsChange: (value: string[]) => void;
  accountIds: string[];
};

export default function Filters({
  onAccountIdsChange,
  accountIds,
}: FiltersProps) {
  return (
    <Box sx={{ display: "flex", flexDirection: "row", gap: 2 }}>
      <AccountsFilter onChange={onAccountIdsChange} values={accountIds} />
      <DescriptionFilter onChange={onDescriptionChange} value={description} />
    </Box>
  );
}
