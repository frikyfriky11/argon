import { Box, Button, ListItem, ListItemProps } from "@mui/material";
import React from "react";
import { Link, useLocation } from "react-router-dom";

export type NavItemProps = {
  href: string;
  icon: React.ReactNode;
  title: string;
};

export const NavItem = ({
  href,
  icon,
  title,
  ...others
}: NavItemProps & ListItemProps) => {
  const router = useLocation();

  const active = href ? router.pathname === href : false;

  return (
    <ListItem
      disableGutters
      sx={{
        display: "flex",
        mb: 0.5,
        py: 0,
        px: 2,
      }}
      {...others}
    >
      <Button
        component={Link}
        disableRipple
        startIcon={icon}
        sx={{
          backgroundColor: active ? "rgba(255,255,255, 0.08)" : "",
          borderRadius: 1,
          color: active ? "secondary.main" : "neutral.300",
          fontWeight: active ? "fontWeightBold" : "fontWeightRegular",
          justifyContent: "flex-start",
          px: 3,
          textAlign: "left",
          textTransform: "none",
          width: "100%",
          "& .MuiButton-startIcon": {
            color: active ? "secondary.main" : "neutral.400",
          },
          "&:hover": {
            backgroundColor: "rgba(255,255,255, 0.08)",
          },
        }}
        to={href}
      >
        <Box sx={{ flexGrow: 1 }}>{title}</Box>
      </Button>
    </ListItem>
  );
};
