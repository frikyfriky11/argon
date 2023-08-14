import MenuIcon from "@mui/icons-material/Menu";
import { AppBar, AppBarProps, Box, IconButton, Toolbar } from "@mui/material";

import LanguageSelector from "./LanguageSelector";

export type DashboardNavbarProps = {
  onSidebarOpen: () => void;
};

export default function Navbar({
  onSidebarOpen,
  ...other
}: DashboardNavbarProps & AppBarProps) {
  return (
    <>
      <AppBar
        sx={{
          left: {
            lg: 280,
          },
          width: {
            lg: "calc(100% - 280px)",
          },
          backgroundColor: "rgba(255, 255, 255, 0.3)",
          backdropFilter: "blur(6px)",
        }}
        {...other}
      >
        <Toolbar
          disableGutters
          sx={{
            minHeight: 64,
            left: 0,
            px: 2,
            justifyContent: "space-between",
          }}
        >
          <Box>
            <IconButton
              onClick={onSidebarOpen}
              sx={{
                display: {
                  xs: "inline-flex",
                  lg: "none",
                },
              }}
            >
              <MenuIcon />
            </IconButton>
          </Box>

          <Box>
            <LanguageSelector />
          </Box>
        </Toolbar>
      </AppBar>
    </>
  );
}
