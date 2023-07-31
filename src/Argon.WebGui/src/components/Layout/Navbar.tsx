import MenuIcon from "@mui/icons-material/Menu";
import { AppBar, AppBarProps, IconButton, Toolbar } from "@mui/material";

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
          }}
        >
          <IconButton
            onClick={onSidebarOpen}
            sx={{
              display: {
                xs: "inline-flex",
                lg: "none",
              },
            }}
          >
            <MenuIcon fontSize="small" />
          </IconButton>
        </Toolbar>
      </AppBar>
    </>
  );
}
