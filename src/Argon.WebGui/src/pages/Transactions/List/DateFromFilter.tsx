import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import { Button, Popover, TextField } from "@mui/material";
import { DatePicker } from "@mui/x-date-pickers";
import { DateTime } from "luxon";
import React, { useState } from "react";

export type DateFromFilterProps = {
  onChange: (value: DateTime | null) => void;
  value: DateTime | null;
};

export default function DateFromFilter({
  onChange,
  value,
}: DateFromFilterProps) {
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const open = Boolean(anchorEl);

  return (
    <div>
      <Button
        endIcon={<ArrowDropDownIcon />}
        onClick={handleClick}
        variant="text"
      >
        Da data {value ? `(!)` : null}
      </Button>
      <Popover
        anchorEl={anchorEl}
        anchorOrigin={{
          vertical: "bottom",
          horizontal: "left",
        }}
        onClose={handleClose}
        open={open}
      >
        <DatePicker onChange={onChange} value={value} />
      </Popover>
    </div>
  );
}
