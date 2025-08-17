import DeleteSweepIcon from "@mui/icons-material/DeleteSweep";
import { Box, Button, Stack } from "@mui/material";

import TextFilter from "../../../components/TextFilter";

export type FiltersProps = {
  onIdentifierTextChange: (value: string) => void;
  identifierText: string | null;
  onClearFilters: () => void;
};

export default function Filters({
  onIdentifierTextChange,
  identifierText,
  onClearFilters,
}: FiltersProps) {
  return (
    <Stack justifyContent="space-between" direction="row" spacing={2}>
      <Box sx={{ display: "flex", flexDirection: "row", flexWrap: "wrap" }}>
        <TextFilter
          label="Nome"
          onChange={onIdentifierTextChange}
          value={identifierText}
        />
      </Box>
      <Stack direction="row" spacing={2}>
        <Button
          variant="text"
          color="error"
          endIcon={<DeleteSweepIcon />}
          onClick={onClearFilters}
          disabled={!identifierText}
        >
          Reimposta
        </Button>
      </Stack>
    </Stack>
  );
}
