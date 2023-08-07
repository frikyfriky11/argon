import { Box } from "@mui/material";
import { DateTime } from "luxon";
import React from "react";

import AccountsFilter from "./AccountsFilter";
import DateFromFilter from "./DateFromFilter";
import DateToFilter from "./DateToFilter";
import DescriptionFilter from "./DescriptionFilter";

export type FiltersProps = {
  onAccountIdsChange: (value: string[]) => void;
  accountIds: string[];
  onDescriptionChange: (value: string | null) => void;
  description: string | null;
  onDateFromChange: (value: DateTime | null) => void;
  dateFrom: DateTime | null;
  onDateToChange: (value: DateTime | null) => void;
  dateTo: DateTime | null;
};

export default function Filters({
  onAccountIdsChange,
  accountIds,
  onDescriptionChange,
  description,
  onDateFromChange,
  dateFrom,
  onDateToChange,
  dateTo,
}: FiltersProps) {
  return (
    <Box sx={{ display: "flex", flexDirection: "row", gap: 2 }}>
      <AccountsFilter onChange={onAccountIdsChange} values={accountIds} />
      <DescriptionFilter onChange={onDescriptionChange} value={description} />
      <DateFromFilter onChange={onDateFromChange} value={dateFrom} />
      <DateToFilter onChange={onDateToChange} value={dateTo} />
    </Box>
  );
}
