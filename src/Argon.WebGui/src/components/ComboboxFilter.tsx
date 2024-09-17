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
  TextField,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { FixedSizeList } from "react-window";

export type ComboboxFilterProps<TValue, TItem> = {
  onChange: (values: TValue[]) => void;
  values: TValue[];
  queryKey: readonly unknown[];
  queryFn: () => TItem[] | Promise<TItem[]>;
  label: string;
  valueSelector: (item: TItem) => TValue;
  labelSelector: (item: TItem) => string;
  showSelectAll?: boolean;
};

export default function ComboboxFilter<TValue, TItem>({
  onChange,
  values,
  queryKey,
  queryFn,
  label,
  valueSelector,
  labelSelector,
  showSelectAll,
}: ComboboxFilterProps<TValue, TItem>) {
  const { t } = useTranslation();

  const [searchFilter, setSearchFilter] = useState("");

  const { data, isPending, isError } = useQuery({
    queryKey,
    queryFn,
  });

  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const open = Boolean(anchorEl);

  const handleToggle = (value: TValue) => () => {
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
    if (data?.length === values.length) {
      onChange([]);
    } else {
      const allValueIds = data?.map(valueSelector);
      onChange(allValueIds ?? []);
    }
  };

  if (isPending) {
    return <p>Loading data...</p>;
  }

  if (isError) {
    return <p>Error while loading data...</p>;
  }

  const filteredData = data.filter((item) =>
    labelSelector(item).toLowerCase().includes(searchFilter.toLowerCase()),
  );

  return (
    <div>
      <Button
        endIcon={<ArrowDropDownIcon />}
        onClick={handleClick}
        variant="text"
      >
        {label} {values.length ? `(${values.length})` : null}
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
            <TextField
              fullWidth
              onChange={(e) => {
                setSearchFilter(e.target.value);
              }}
              placeholder="Ricerca..."
              size="small"
              sx={{ mb: 1 }}
              value={searchFilter}
            />
          </ListItem>
          {showSelectAll && (
            <ListItem disablePadding>
              <ListItemButton dense onClick={handleSelectAllToggle}>
                <ListItemIcon>
                  <Checkbox
                    checked={data.length === values.length}
                    disableRipple
                    edge="start"
                    indeterminate={
                      values.length > 0 && data.length !== values.length
                    }
                    tabIndex={-1}
                  />
                </ListItemIcon>
                <ListItemText primary="Seleziona tutto" />
              </ListItemButton>
            </ListItem>
          )}
          <FixedSizeList
            height={360}
            itemCount={filteredData.length}
            itemSize={50}
            overscanCount={5}
            width={300}
          >
            {({ index, style }) => (
              <ListItem disablePadding key={index} style={style}>
                <ListItemButton
                  dense
                  onClick={handleToggle(valueSelector(filteredData[index]))}
                >
                  <ListItemIcon>
                    <Checkbox
                      checked={values.includes(
                        valueSelector(filteredData[index]),
                      )}
                      disableRipple
                      edge="start"
                      tabIndex={-1}
                    />
                  </ListItemIcon>
                  <ListItemText primary={labelSelector(filteredData[index])} />
                </ListItemButton>
              </ListItem>
            )}
          </FixedSizeList>
        </List>
      </Popover>
    </div>
  );
}
