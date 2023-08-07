import { Box } from "@mui/material";
import React from "react";

import AccountsFilter from "./AccountsFilter";
import DescriptionFilter from "./DescriptionFilter";

export type FiltersProps = {
  onAccountIdsChange: (value: string[]) => void;
  accountIds: string[];
  onDescriptionChange: (value: string | null) => void;
  description: string | null;
};

export default function Filters({
  onAccountIdsChange,
  accountIds,
  onDescriptionChange,
  description,
}: FiltersProps) {
  return (
    <Box sx={{ display: "flex", flexDirection: "row", gap: 2 }}>
      <AccountsFilter onChange={onAccountIdsChange} values={accountIds} />
      <DescriptionFilter onChange={onDescriptionChange} value={description} />
    </Box>
  );
}
