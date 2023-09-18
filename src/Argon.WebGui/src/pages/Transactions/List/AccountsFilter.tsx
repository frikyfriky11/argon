import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import {
  Button,
  Checkbox,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Popover,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import React, { useState } from "react";
import { FixedSizeList } from "react-window";

import { AccountsClient } from "../../../services/backend/BackendClient";

export type AccountsFilterProps = {
  onChange: (values: string[]) => void;
  values: string[];
};

export default function AccountsFilter({
  onChange,
  values,
}: AccountsFilterProps) {
  const accounts = useQuery({
    queryKey: ["accounts"],
    queryFn: () => new AccountsClient().getList(null, null),
  });

  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const open = Boolean(anchorEl);

  const handleToggle = (value: string) => () => {
    const currentIndex = values.indexOf(value);
    const newChecked = [...values];

    if (currentIndex === -1) {
      newChecked.push(value);
    } else {
      newChecked.splice(currentIndex, 1);
    }

    onChange(newChecked);
  };

  const handleSelectAllToggle = () => {
    if (accounts.data?.length === values.length) {
      onChange([]);
    } else {
      const allAccountIds = accounts.data?.map((account) => account.id);
      onChange(allAccountIds ?? []);
    }
  };

  if (accounts.isLoading) {
    return <p>Loading accounts...</p>;
  }

  if (accounts.isError) {
    return <p>Error while loading accounts...</p>;
  }

  return (
    <div>
      <Button
        endIcon={<ArrowDropDownIcon />}
        onClick={handleClick}
        variant="text"
      >
        Conti {values.length ? `(${values.length})` : null}
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
        <List disablePadding>
          <ListItem disablePadding>
            <ListItemButton dense onClick={handleSelectAllToggle}>
              <ListItemIcon>
                <Checkbox
                  checked={accounts.data.length === values.length}
                  disableRipple
                  edge="start"
                  indeterminate={
                    values.length > 0 && accounts.data.length !== values.length
                  }
                  tabIndex={-1}
                />
              </ListItemIcon>
              <ListItemText primary={"Select All"} />
            </ListItemButton>
          </ListItem>
          <FixedSizeList
            height={360}
            itemCount={accounts.data.length}
            itemSize={50}
            overscanCount={5}
            width={300}
          >
            {({ index, style }) => (
              <ListItem disablePadding key={index} style={style}>
                <ListItemButton
                  dense
                  onClick={handleToggle(accounts.data[index].id)}
                >
                  <ListItemIcon>
                    <Checkbox
                      checked={values.includes(accounts.data[index].id)}
                      disableRipple
                      edge="start"
                      tabIndex={-1}
                    />
                  </ListItemIcon>
                  <ListItemText primary={accounts.data[index].name} />
                </ListItemButton>
              </ListItem>
            )}
          </FixedSizeList>
        </List>
      </Popover>
    </div>
  );
}
