import AccountBalanceWalletIcon from "@mui/icons-material/AccountBalanceWallet";
import BarChartIcon from "@mui/icons-material/BarChart";
import FolderIcon from "@mui/icons-material/Folder";
import ReceiptLongIcon from "@mui/icons-material/ReceiptLong";
import {
  Box,
  Divider,
  Drawer,
  Stack,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import React from "react";
import { Link } from "react-router-dom";

import Logo from "../../assets/logo.png";
import { NavItem } from "./NavItem";

const items = [
  {
    href: "/dashboard",
    icon: <BarChartIcon fontSize="small" />,
    title: "Dashboard",
  },
  {
    href: "/accounts",
    icon: <FolderIcon fontSize="small" />,
    title: "Conti",
  },
  {
    href: "/transactions",
    icon: <ReceiptLongIcon fontSize="small" />,
    title: "Transazioni",
  },
  {
    href: "/budget",
    icon: <AccountBalanceWalletIcon fontSize="small" />,
    title: "Budget per conto",
  },
];

export type DashboardSidebarProps = {
  open: boolean;

  onClose: () => void;
};

export default function Sidebar({ onClose, open }: DashboardSidebarProps) {
  const theme = useTheme();

  const lgUp = useMediaQuery(theme.breakpoints.up("lg"), {
    defaultMatches: true,
    noSsr: false,
  });

  const content = (
    <>
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          height: "100%",
        }}
      >
        <Stack
          alignItems="center"
          justifyContent="center"
          spacing={2}
          sx={{ p: 4 }}
        >
          <Link to="/">
            <img height={64} loading="lazy" src={Logo} width={64} />
          </Link>
          <Typography variant="h5">Argon</Typography>
        </Stack>
        <Divider
          sx={{
            borderColor: "#2D3748",
            mb: 3,
          }}
        />
        <Box sx={{ flexGrow: 1 }}>
          {items.map((item) => (
            <NavItem
              href={item.href}
              icon={item.icon}
              key={item.title}
              title={item.title}
            />
          ))}
        </Box>
      </Box>
    </>
  );

  if (lgUp) {
    return (
      <Drawer
        PaperProps={{
          sx: {
            backgroundColor: "neutral.800",
            color: "#FFFFFF",
            width: 280,
          },
        }}
        anchor="left"
        open
        variant="permanent"
      >
        {content}
      </Drawer>
    );
  }

  return (
    <Drawer
      PaperProps={{
        sx: {
          backgroundColor: "neutral.800",
          color: "#FFFFFF",
          width: 280,
        },
      }}
      anchor="left"
      onClose={onClose}
      open={open}
      sx={{ zIndex: (theme) => theme.zIndex.appBar + 100 }}
      variant="temporary"
    >
      {content}
    </Drawer>
  );
}
