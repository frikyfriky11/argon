import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import DeleteIcon from "@mui/icons-material/Delete";
import {
  Button,
  ButtonProps,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  useTheme,
} from "@mui/material";
import { useState } from "react";

export type ActionMenuProps = {
  onDelete: () => void;
};

export default function ActionMenu({
  onDelete,
  ...other
}: ActionMenuProps & ButtonProps) {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const open = Boolean(anchorEl);

  const theme = useTheme();

  return (
    <>
      <Button
        endIcon={<ArrowDropDownIcon />}
        onClick={(e) => {
          setAnchorEl(e.currentTarget);
        }}
        variant="contained"
        {...other}
      >
        Azioni
      </Button>
      <Menu
        anchorEl={anchorEl}
        onClose={() => {
          setAnchorEl(null);
        }}
        open={open}
      >
        <MenuItem
          onClick={() => {
            setAnchorEl(null);
            onDelete();
          }}
          sx={{ color: theme.palette.error.main }}
        >
          <ListItemIcon>
            <DeleteIcon color="error" />
          </ListItemIcon>
          <ListItemText>Elimina</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
}
