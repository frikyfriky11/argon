import CloseIcon from "@mui/icons-material/Close";
import DeleteSweepIcon from "@mui/icons-material/DeleteSweep";
import {
  AppBar,
  Box,
  Button,
  Dialog,
  Grid,
  IconButton,
  Slide,
  Stack,
  Toolbar,
  Typography,
} from "@mui/material";
import { TransitionProps } from "@mui/material/transitions";
import { DateTime } from "luxon";
import { ReactElement, Ref, forwardRef, useState } from "react";

import ComboboxFilter from "../../../components/ComboboxFilter";
import DateFilter from "../../../components/DateFilter";
import {
  AccountsClient,
  CounterpartiesClient,
} from "../../../services/backend/BackendClient";

const Transition = forwardRef(function Transition(
  props: TransitionProps & {
    children: ReactElement;
  },
  ref: Ref<unknown>,
) {
  return <Slide direction="up" ref={ref} {...props} />;
});

export type FiltersDialogProps = {
  open: boolean;
  onClose: () => void;
  onApply: (filters: {
    accountIds: string[];
    counterpartyIds: string[];
    dateFrom: DateTime | null;
    dateTo: DateTime | null;
  }) => void;
  initialFilters: {
    accountIds: string[];
    counterpartyIds: string[];
    dateFrom: DateTime | null;
    dateTo: DateTime | null;
  };
  onClear: () => void;
};

export default function FiltersDialog({
  open,
  onClose,
  onApply,
  initialFilters,
  onClear,
}: FiltersDialogProps) {
  const [filters, setFilters] = useState(initialFilters);

  const handleApply = () => {
    onApply(filters);
    onClose();
  };

  const handleClear = () => {
    onClear();
    onClose();
  };

  return (
    <Dialog
      fullScreen
      open={open}
      onClose={onClose}
      TransitionComponent={Transition}
    >
      <AppBar sx={{ position: "relative" }}>
        <Toolbar>
          <IconButton
            edge="start"
            color="inherit"
            onClick={onClose}
            aria-label="close"
          >
            <CloseIcon />
          </IconButton>
          <Typography sx={{ ml: 2, flex: 1 }} variant="h6" component="div">
            Filtri
          </Typography>
        </Toolbar>
      </AppBar>
      <Box sx={{ p: 2, pb: "100px" }}>
        <Stack spacing={2}>
          <ComboboxFilter
            label={"Conti"}
            labelSelector={(item) => item.name}
            onChange={(accountIds) => {
              setFilters((prev) => ({ ...prev, accountIds }));
            }}
            queryFn={() => new AccountsClient().getList(null, null)}
            queryKey={["accounts"]}
            valueSelector={(item) => item.id}
            values={filters.accountIds}
          />
          <ComboboxFilter
            label={"Controparti"}
            labelSelector={(item) => item.name}
            onChange={(counterpartyIds) => {
              setFilters((prev) => ({ ...prev, counterpartyIds }));
            }}
            queryFn={async () =>
              (await new CounterpartiesClient().getList(null, 1, 10_000)).items
            }
            queryKey={["counterparties"]}
            valueSelector={(item) => item.id}
            values={filters.counterpartyIds}
          />
          <DateFilter
            label="Da data"
            onChange={(dateFrom) => {
              setFilters((prev) => ({ ...prev, dateFrom }));
            }}
            value={filters.dateFrom}
          />
          <DateFilter
            label="A data"
            onChange={(dateTo) => {
              setFilters((prev) => ({ ...prev, dateTo }));
            }}
            value={filters.dateTo}
          />
        </Stack>
      </Box>
      <Box
        sx={{
          position: "fixed",
          bottom: 0,
          left: 0,
          right: 0,
          p: 2,
          bgcolor: "background.paper",
          borderTop: "1px solid",
          borderColor: "divider",
        }}
      >
        <Grid
          container
          spacing={2}
          justifyContent={{ xs: "center", sm: "flex-end" }}
        >
          <Grid item xs={6} sm="auto">
            <Button
              variant="text"
              color="error"
              endIcon={<DeleteSweepIcon />}
              onClick={handleClear}
              sx={{ width: { xs: "100%", sm: "auto" } }}
            >
              Reimposta
            </Button>
          </Grid>
          <Grid item xs={6} sm="auto">
            <Button
              onClick={handleApply}
              variant={"contained"}
              sx={{ width: { xs: "100%", sm: "auto" } }}
            >
              Applica
            </Button>
          </Grid>
        </Grid>
      </Box>
    </Dialog>
  );
}
