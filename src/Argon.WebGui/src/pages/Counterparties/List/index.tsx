import { Stack } from "@mui/material";
import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { CounterpartiesClient } from "../../../services/backend/BackendClient";
import useSearchParamsState from "../../../utils/UrlUtils";
import Filters from "./Filters.tsx";
import ResultsAsTable from "./ResultsAsTable.tsx";

export default function Index() {
  const [filters, setFilters] = useSearchParamsState({
    page: 0,
    pageSize: 10,
    name: "",
  });

  const clearFilters = () => {
    setFilters((prev) => ({
      ...prev,
      page: 0,
      name: "",
    }));
  };

  const counterparties = useQuery({
    queryKey: ["counterparties", filters.name, filters.page, filters.pageSize],
    queryFn: () =>
      new CounterpartiesClient().getList(
        filters.name,
        filters.page + 1,
        filters.pageSize,
      ),
    placeholderData: keepPreviousData,
  });

  if (counterparties.isPending) {
    return <p>Loading counterparties...</p>;
  }

  if (counterparties.isError) {
    return <p>Error while loading counterparties...</p>;
  }

  return (
    <Stack spacing={4}>
      <Filters
        name={filters.name}
        onNameChange={(name) => {
          setFilters((prev) => ({ ...prev, name }));
        }}
        onClearFilters={clearFilters}
      />
      <ResultsAsTable
        onPageChange={(page) => {
          setFilters((prev) => ({ ...prev, page }));
        }}
        onPageSizeChange={(pageSize) => {
          setFilters((prev) => ({ ...prev, pageSize }));
        }}
        page={filters.page}
        rowsPerPage={filters.pageSize}
        totalRows={counterparties.data.totalCount}
        counterparties={counterparties.data.items}
      />
    </Stack>
  );
}
