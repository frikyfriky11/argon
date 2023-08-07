import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import { Button, Popover, TextField } from "@mui/material";
import React, { useState } from "react";

export type DescriptionFilterProps = {
  onChange: (value: string | null) => void;
  value: string | null;
};

export default function DescriptionFilter({
  onChange,
  value,
}: DescriptionFilterProps) {
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
        Descrizione {value?.length ? `(!)` : null}
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
        <TextField
          onChange={(e) => {
            onChange(e.target.value !== "" ? e.target.value : null);
          }}
          value={value ?? ""}
        />
      </Popover>
    </div>
  );
}
