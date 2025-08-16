import ArrowDownwardIcon from "@mui/icons-material/ArrowDownward";
import EditIcon from "@mui/icons-material/Edit";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  CircularProgress,
  Collapse,
  IconButton,
  ListItemText,
  Stack,
  Typography,
} from "@mui/material";
import { blue, green, grey, red } from "@mui/material/colors";
import {
  InfiniteData,
  InfiniteQueryObserverResult,
} from "@tanstack/react-query";
import { DateTime } from "luxon";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useInView } from "react-intersection-observer";
import { Link } from "react-router-dom";

import {
  AccountType,
  ITransactionRowsGetListResponse,
  ITransactionsGetListResponse,
  PaginatedListOfTransactionsGetListResponse,
} from "../../../services/backend/BackendClient";

export type ResultsAsFeedProps = {
  transactions?: ITransactionsGetListResponse[];
  fetchNextPage: () => Promise<
    InfiniteQueryObserverResult<
      InfiniteData<PaginatedListOfTransactionsGetListResponse>
    >
  >;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
};

function TransactionRow({ row }: { row: ITransactionRowsGetListResponse }) {
  const { i18n } = useTranslation();

  const isDebit = row.debit !== null;
  const amount = (isDebit ? row.debit : row.credit)?.toLocaleString(
    i18n.language,
    {
      style: "currency",
      currency: "EUR",
    },
  );

  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
      }}
    >
      <Box sx={{ flexGrow: 1 }}>
        <ListItemText primary={row.accountName} secondary={row.description} />
      </Box>
      <Typography sx={{ fontFamily: "monospace", pl: 1 }}>{amount}</Typography>
    </Box>
  );
}

function TransactionCard({
  transaction,
}: {
  transaction: ITransactionsGetListResponse;
}) {
  const { i18n } = useTranslation();
  const [expanded, setExpanded] = useState(false);

  const debits = transaction.transactionRows.filter(
    (row) => row.debit !== null,
  );
  const credits = transaction.transactionRows.filter(
    (row) => row.credit !== null,
  );

  const isExpense = debits.some(
    (row) => row.accountType === AccountType.Expense,
  );
  const isRevenue = credits.some(
    (row) => row.accountType === AccountType.Revenue,
  );

  const total = (isExpense ? debits : credits)
    .reduce((acc, row) => acc + (row.debit ?? row.credit ?? 0), 0)
    .toLocaleString(i18n.language, {
      style: "currency",
      currency: "EUR",
    });

  let summaryText = total;
  let summaryColor: string = blue[500];

  if (isExpense) {
    summaryText = `- ${total}`;
    summaryColor = red[500];
  } else if (isRevenue) {
    summaryText = `+ ${total}`;
    summaryColor = green[500];
  }

  return (
    <Card key={transaction.id}>
      <CardHeader
        action={
          <Box>
            <IconButton
              component={Link}
              to={`/transactions/${transaction.id}`}
              aria-label="edit"
            >
              <EditIcon />
            </IconButton>
            <IconButton
              onClick={() => {
                setExpanded(!expanded);
              }}
              aria-expanded={expanded}
              aria-label="show more"
            >
              <ExpandMoreIcon />
            </IconButton>
          </Box>
        }
        title={transaction.counterpartyName}
        subheader={transaction.date
          .setLocale(i18n.language)
          .toLocaleString(DateTime.DATE_MED)}
        sx={{ p: 2, pb: 1 }}
      />
      <CardContent sx={{ pt: 0, p: 2, "&:last-child": { pb: 2 } }}>
        <Typography
          variant="h5"
          component="div"
          sx={{ mb: 2, color: summaryColor }}
        >
          {summaryText}
        </Typography>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Box
            sx={{
              backgroundColor: grey[100],
              borderRadius: 1,
              p: 2,
            }}
          >
            <Stack spacing={1}>
              {credits.map((row) => (
                <TransactionRow key={row.id} row={row} />
              ))}
              <Box sx={{ display: "flex", justifyContent: "center", my: 1 }}>
                <ArrowDownwardIcon />
              </Box>
              {debits.map((row) => (
                <TransactionRow key={row.id} row={row} />
              ))}
            </Stack>
          </Box>
        </Collapse>
      </CardContent>
    </Card>
  );
}

export default function ResultsAsFeed({
  transactions,
  fetchNextPage,
  hasNextPage,
  isFetchingNextPage,
}: ResultsAsFeedProps) {
  const { ref, inView } = useInView();

  useEffect(() => {
    if (inView) {
      void fetchNextPage();
    }
  }, [inView, fetchNextPage]);

  return (
    <Stack spacing={1}>
      {transactions?.map((transaction) => (
        <TransactionCard key={transaction.id} transaction={transaction} />
      ))}
      <Box ref={ref}>
        {isFetchingNextPage && (
          <Box justifyContent="center" p={2} sx={{ display: "flex" }}>
            <CircularProgress />
          </Box>
        )}

        {!hasNextPage && (
          <Box justifyContent="center" p={2} sx={{ display: "flex" }}>
            <Typography fontSize="larger">No more results</Typography>
          </Box>
        )}
      </Box>
    </Stack>
  );
}
