import { Stack } from "@mui/material";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";

import { CounterpartyIdentifiersClient } from "../../../services/backend/BackendClient";
import useSearchParamsState from "../../../utils/UrlUtils";
import Filters from "./Filters.tsx";
import ResultsAsTable from "./ResultsAsTable.tsx";
import Toolbar from "./Toolbar.tsx";

export default function Index() {
  const { counterpartyId } = useParams() as { counterpartyId: string };

  const [filters, setFilters] = useSearchParamsState({
    page: 0,
    pageSize: 10,
    counterpartyId,
    identifierText: "",
  });

  const clearFilters = () => {
    setFilters((prev) => ({
      ...prev,
      page: 0,
      counterpartyId,
      identifierText: "",
    }));
  };

  const counterpartyIdentifiers = useQuery({
    queryKey: [
      "counterpartyIdentifiers",
      filters.counterpartyId,
      filters.identifierText,
      filters.page,
      filters.pageSize,
    ],
    queryFn: () =>
      new CounterpartyIdentifiersClient().getList(
        filters.counterpartyId,
        filters.identifierText,
        filters.page + 1,
        filters.pageSize,
      ),
    placeholderData: keepPreviousData,
  });

  if (counterpartyIdentifiers.isPending) {
    return <p>Loading counterparty identifiers...</p>;
  }

  if (counterpartyIdentifiers.isError) {
    return <p>Error while loading counterparty identifiers...</p>;
  }

  return (
    <Stack spacing={4}>
      <Toolbar counterpartyId={counterpartyId} />
      <Filters
        identifierText={filters.identifierText}
        onIdentifierTextChange={(identifierText) => {
          setFilters((prev) => ({ ...prev, identifierText }));
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
        totalRows={counterpartyIdentifiers.data.totalCount}
        counterpartyIdentifiers={counterpartyIdentifiers.data.items}
      />
    </Stack>
  );
}
