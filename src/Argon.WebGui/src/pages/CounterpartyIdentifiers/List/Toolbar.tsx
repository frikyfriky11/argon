import AddIcon from "@mui/icons-material/Add";
import { Box, BoxProps, Button, Typography } from "@mui/material";
import { Link } from "react-router-dom";

export type ToolbarProps = {
  counterpartyId: string;
};

export default function Toolbar({
  counterpartyId,
  ...other
}: ToolbarProps & BoxProps) {
  return (
    <Box {...other}>
      <Box
        justifyContent="space-between"
        sx={{
          alignItems: "center",
          display: "flex",
          flexWrap: "wrap",
        }}
      >
        <Typography variant="h4">Nomi alternativi</Typography>
        <Box>
          <Button
            color="primary"
            component={Link}
            startIcon={<AddIcon />}
            to={`/counterparties/${counterpartyId}/identifiers/add`}
            variant="contained"
          >
            Nuovo nome alternativo
          </Button>
        </Box>
      </Box>
    </Box>
  );
}
