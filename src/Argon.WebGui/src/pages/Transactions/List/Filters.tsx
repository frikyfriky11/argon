import DeleteSweepIcon from "@mui/icons-material/DeleteSweep";
import { Box, Button, Stack } from "@mui/material";
import { DateTime } from "luxon";

import ComboboxFilter from "../../../components/ComboboxFilter";
import DateFilter from "../../../components/DateFilter";
import TextFilter from "../../../components/TextFilter";
import { AccountsClient } from "../../../services/backend/BackendClient";

export type FiltersProps = {
  onAccountIdsChange: (value: string[]) => void;
  accountIds: string[];
  onDescriptionChange: (value: string) => void;
  description: string | null;
  onDateFromChange: (value: DateTime | null) => void;
  dateFrom: DateTime | null;
  onDateToChange: (value: DateTime | null) => void;
  dateTo: DateTime | null;
  onClearFilters: () => void;
};

export default function Filters({
  onAccountIdsChange,
  accountIds,
  onDescriptionChange,
  description,
  onDateFromChange,
  dateFrom,
  onDateToChange,
  dateTo,
  onClearFilters,
}: FiltersProps) {
  return (
    <Stack justifyContent="space-between" direction="row" spacing={2}>
      <Box sx={{ display: "flex", flexDirection: "row", flexWrap: "wrap" }}>
        <ComboboxFilter
          label={"Conti"}
          labelSelector={(item) => item.name}
          onChange={onAccountIdsChange}
          queryFn={() => new AccountsClient().getList(null, null)}
          queryKey={["accounts"]}
          valueSelector={(item) => item.id}
          values={accountIds}
        />
        <TextFilter
          label="Descrizione"
          onChange={onDescriptionChange}
          value={description}
        />
        <DateFilter
          label="Da data"
          onChange={onDateFromChange}
          value={dateFrom}
        />
        <DateFilter label="A data" onChange={onDateToChange} value={dateTo} />
      </Box>
      <Stack direction="row" spacing={2}>
        <Button
          variant="text"
          color="error"
          endIcon={<DeleteSweepIcon />}
          onClick={onClearFilters}
          disabled={
            !(accountIds.length > 0 || !!description || !!dateFrom || !!dateTo)
          }
        >
          Reimposta
        </Button>
      </Stack>
    </Stack>
  );
}
