import { Box, BoxProps, Typography } from "@mui/material";

export default function Toolbar(props: BoxProps) {
  return (
    <Box {...props}>
      <Box
        justifyContent="space-between"
        sx={{
          alignItems: "center",
          display: "flex",
          flexWrap: "wrap",
        }}
      >
        <Typography variant="h4">Budget per conto</Typography>
      </Box>
    </Box>
  );
}
