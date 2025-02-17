import DeleteSweepIcon from "@mui/icons-material/DeleteSweep";
import { Box, Button, Stack } from "@mui/material";

import TextFilter from "../../../components/TextFilter";

export type FiltersProps = {
  onNameChange: (value: string) => void;
  name: string | null;
  onClearFilters: () => void;
};

export default function Filters({
  onNameChange,
  name,
  onClearFilters,
}: FiltersProps) {
  return (
    <Stack justifyContent="space-between" direction="row" spacing={2}>
      <Box sx={{ display: "flex", flexDirection: "row", flexWrap: "wrap" }}>
        <TextFilter label="Nome" onChange={onNameChange} value={name} />
      </Box>
      <Stack direction="row" spacing={2}>
        <Button
          variant="text"
          color="error"
          endIcon={<DeleteSweepIcon />}
          onClick={onClearFilters}
          disabled={!name}
        >
          Reimposta
        </Button>
      </Stack>
    </Stack>
  );
}
