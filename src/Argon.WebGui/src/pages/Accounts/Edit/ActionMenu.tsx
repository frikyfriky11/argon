import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import DeleteIcon from "@mui/icons-material/Delete";
import StarIcon from "@mui/icons-material/Star";
import {
  Button,
  ButtonProps,
  Divider,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  useTheme,
} from "@mui/material";
import { useState } from "react";

export type ActionMenuProps = {
  onDelete: () => void;
  isFavourite: boolean;
  onFavourite: () => void;
};

export default function ActionMenu({
  onDelete,
  isFavourite,
  onFavourite,
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
            onFavourite();
          }}
        >
          <ListItemIcon>
            <StarIcon />
          </ListItemIcon>
          <ListItemText>
            {isFavourite ? "Rimuovi dai preferiti" : "Aggiungi ai preferiti"}
          </ListItemText>
        </MenuItem>
        <Divider />
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
