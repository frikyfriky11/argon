import AddIcon from "@mui/icons-material/Add";
import { Box, BoxProps, Button, Stack, Typography } from "@mui/material";
import { Link } from "react-router-dom";

export default function Toolbar({ ...props }: BoxProps) {
  return (
    <Box {...props}>
      <Box
        gap={2}
        justifyContent="space-between"
        sx={{
          alignItems: "center",
          display: "flex",
          flexWrap: "wrap",
        }}
      >
        <Stack direction="row" gap={2}>
          <Typography variant="h4">Transazioni</Typography>
        </Stack>
        <Stack direction="row" gap={2}>
          <Button
            color="primary"
            component={Link}
            startIcon={<AddIcon />}
            to="/transactions/add"
            variant="contained"
          >
            Nuova transazione
          </Button>
        </Stack>
      </Box>
    </Box>
  );
}
