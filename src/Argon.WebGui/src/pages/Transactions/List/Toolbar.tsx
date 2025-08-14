import AddIcon from "@mui/icons-material/Add";
import SegmentIcon from "@mui/icons-material/Segment";
import TableRowsIcon from "@mui/icons-material/TableRows";
import ViewStreamIcon from "@mui/icons-material/ViewStream";
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
  selectedView: "table" | "journal" | "feed";
  onSelectedViewChange: (value: "table" | "journal" | "feed") => void;
};

export default function Toolbar({
  selectedView,
  onSelectedViewChange,
  ...props
}: ToolbarProps & BoxProps) {
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
          <ToggleButtonGroup
            color="primary"
            exclusive
            onChange={(_, value) => {
              onSelectedViewChange(
                (value as "table" | "journal" | "feed" | null) ?? "table",
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
            <ToggleButton value={"feed"}>
              <Tooltip title="Feed">
                <ViewStreamIcon />
              </Tooltip>
            </ToggleButton>
          </ToggleButtonGroup>
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
