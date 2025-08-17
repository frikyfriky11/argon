import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import DeleteIcon from "@mui/icons-material/Delete";
import StorefrontIcon from "@mui/icons-material/Storefront";
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
import { Link } from "react-router-dom";

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
        <MenuItem component={Link} to={"identifiers"}>
          <ListItemIcon>
            <StorefrontIcon />
          </ListItemIcon>
          <ListItemText>Nomi alternativi</ListItemText>
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
