import { Box, Container, useTheme } from "@mui/material";
import React, { Suspense } from "react";
import { Outlet } from "react-router-dom";

export default function MainContent() {
  const theme = useTheme();

  return (
    <Box
      sx={{
        display: "flex",
        flex: "1 1 auto",
        flexDirection: "column",
        width: "100%",
        maxWidth: "100%",
        pt: "64px",
        [theme.breakpoints.up("lg")]: {
          paddingLeft: "280px",
        },
      }}
    >
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          py: 2,
        }}
      >
        <Container>
          <Suspense fallback={<p>Route is loading...</p>}>
            <Outlet />
          </Suspense>
        </Container>
      </Box>
    </Box>
  );
}
