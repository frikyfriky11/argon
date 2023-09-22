import { Box } from "@mui/material";
import { DateTime } from "luxon";
import React from "react";

import DateFilter from "../../../components/DateFilter";

export type FiltersProps = {
  onDateChange: (value: DateTime | null) => void;
  date: DateTime | null;
};

export default function Filters({ onDateChange, date }: FiltersProps) {
  return (
    <Box sx={{ display: "flex", flexDirection: "row", flexWrap: "wrap" }}>
      <DateFilter
        label="Periodo"
        onChange={onDateChange}
        value={date}
        views={["month", "year"]}
      />
    </Box>
  );
}
