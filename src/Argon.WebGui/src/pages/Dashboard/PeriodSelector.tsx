import { ToggleButton, ToggleButtonGroup } from "@mui/material";
import React from "react";

import { PeriodPreset } from "../../utils/statistics";

export type PeriodSelectorProps = {
  value: PeriodPreset;
  onChange: (value: PeriodPreset) => void;
};

const options: { value: PeriodPreset; label: string }[] = [
  { value: "12m", label: "12 mesi" },
  { value: "ytd", label: "Anno in corso" },
  { value: "all", label: "Tutto" },
];

export default function PeriodSelector({
  value,
  onChange,
}: PeriodSelectorProps) {
  return (
    <ToggleButtonGroup
      color="primary"
      exclusive
      onChange={(_, next: PeriodPreset | null) => {
        if (next !== null) {
          onChange(next);
        }
      }}
      size="small"
      value={value}
    >
      {options.map((option) => (
        <ToggleButton key={option.value} value={option.value}>
          {option.label}
        </ToggleButton>
      ))}
    </ToggleButtonGroup>
  );
}
