import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import { Button, Popover } from "@mui/material";
import { DatePicker, DateView } from "@mui/x-date-pickers";
import { DateTime } from "luxon";
import React, { useState } from "react";

export type DateFilterProps = {
  onChange: (value: DateTime | null) => void;
  value: DateTime | null;
  label: string;
  views?: DateView[];
};

export default function DateFilter({
  onChange,
  value,
  label,
  views,
}: DateFilterProps) {
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
        {label} {value ? `(!)` : null}
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
        <DatePicker onChange={onChange} value={value} views={views} />
      </Popover>
    </div>
  );
}
