import { Box } from "@mui/material";
import { DateTime } from "luxon";
import React from "react";

import ComboboxFilter from "../../../components/ComboboxFilter";
import DateFilter from "../../../components/DateFilter";
import TextFilter from "../../../components/TextFilter";
import { AccountsClient } from "../../../services/backend/BackendClient";

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
    <Box sx={{ display: "flex", flexDirection: "row", flexWrap: "wrap" }}>
      <ComboboxFilter
        label={"Conti"}
        labelSelector={(item) => item.name}
        onChange={onAccountIdsChange}
        queryFn={() => new AccountsClient().getList(null, null)}
        queryKey={["accounts"]}
        valueSelector={(item) => item.id}
        values={accountIds}
      />
      <TextFilter
        label="Descrizione"
        onChange={onDescriptionChange}
        value={description}
      />
      <DateFilter
        label="Da data"
        onChange={onDateFromChange}
        value={dateFrom}
      />
      <DateFilter label="A data" onChange={onDateToChange} value={dateTo} />
    </Box>
  );
}
