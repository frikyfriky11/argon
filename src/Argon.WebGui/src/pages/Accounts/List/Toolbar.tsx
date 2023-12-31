import AddIcon from "@mui/icons-material/Add";
import { Box, BoxProps, Button, Typography } from "@mui/material";
import { Link } from "react-router-dom";

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
        <Typography variant="h4">Conti</Typography>
        <Box>
          <Button
            color="primary"
            component={Link}
            startIcon={<AddIcon />}
            to="/accounts/add"
            variant="contained"
          >
            Nuovo conto
          </Button>
        </Box>
      </Box>
    </Box>
  );
}
