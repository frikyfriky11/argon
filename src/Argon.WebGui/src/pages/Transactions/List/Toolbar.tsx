import AddIcon from "@mui/icons-material/Add";
import SegmentIcon from "@mui/icons-material/Segment";
import TableRowsIcon from "@mui/icons-material/TableRows";
import {
  Box,
  BoxProps,
  Button,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
  Typography,
} from "@mui/material";
import { Link } from "react-router-dom";

export type ToolbarProps = {
  selectedView: "table" | "journal";
  onSelectedViewChange: (value: "table" | "journal") => void;
};

export default function Toolbar({
  selectedView,
  onSelectedViewChange,
  ...props
}: ToolbarProps & BoxProps) {
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
        <Typography variant="h4">Transazioni</Typography>
        <Stack direction="row" gap={2}>
          <ToggleButtonGroup
            color="primary"
            exclusive
            onChange={(_, value) => {
              onSelectedViewChange(
                (value as "table" | "journal" | null) ?? "table",
              );
            }}
            size="small"
            value={selectedView}
          >
            <ToggleButton value={"table"}>
              <Tooltip title="Tabella">
                <TableRowsIcon />
              </Tooltip>
            </ToggleButton>
            <ToggleButton value={"journal"}>
              <Tooltip title="Giornale">
                <SegmentIcon />
              </Tooltip>
            </ToggleButton>
          </ToggleButtonGroup>
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
