import ArrowDownwardIcon from "@mui/icons-material/ArrowDownward";
import {
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  ListItemText,
  Stack,
  Typography,
} from "@mui/material";
import { grey } from "@mui/material/colors";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { DateTime } from "luxon";
import { enqueueSnackbar } from "notistack";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

import {
  ITransactionRowsGetListResponse,
  ITransactionsGetListResponse,
  TransactionsClient,
} from "../../../services/backend/BackendClient.ts";

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

export type DuplicateResolverProps = {
  transaction: ITransactionsGetListResponse;
};

function TransactionCard({
  title,
  transaction,
}: {
  title: string;
  transaction: ITransactionsGetListResponse;
}) {
  const { i18n } = useTranslation();

  const debits = transaction.transactionRows.filter(
    (row) => row.debit !== null,
  );
  const credits = transaction.transactionRows.filter(
    (row) => row.credit !== null,
  );

  return (
    <Card sx={{ height: "100%" }}>
      <CardContent>
        <Stack spacing={2}>
          <Typography variant="h6">{title}</Typography>
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="body2" color="text.secondary">
              Data
            </Typography>
            <Typography variant="body2">
              {transaction.date
                .setLocale(i18n.language)
                .toLocaleString(DateTime.DATE_MED)}
            </Typography>
          </Stack>
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="body2" color="text.secondary">
              Controparte
            </Typography>
            <Typography variant="body2">
              {transaction.counterpartyName}
            </Typography>
          </Stack>
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="body2" color="text.secondary">
              Importo
            </Typography>
            <Typography variant="body2" sx={{ fontFamily: "monospace" }}>
              {transaction.transactionRows
                .filter((row) => row.debit !== null)
                .reduce((acc, row) => acc + (row.debit ?? 0), 0)
                .toLocaleString("it-IT", {
                  style: "currency",
                  currency: "EUR",
                })}
            </Typography>
          </Stack>
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
        </Stack>
      </CardContent>
    </Card>
  );
}

export default function DuplicateResolver({
  transaction,
}: DuplicateResolverProps) {
  const queryClient = useQueryClient();

  const originalTransaction = useQuery({
    queryKey: ["transactions", transaction.potentialDuplicateOfTransactionId],
    queryFn: () =>
      new TransactionsClient().get(
        transaction.potentialDuplicateOfTransactionId!,
      ),
    enabled: !!transaction.potentialDuplicateOfTransactionId,
  });

  const deleteMutation = useMutation({
    mutationFn: () => new TransactionsClient().delete(transaction.id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["bankStatements"] });
      enqueueSnackbar("Transazione duplicata eliminata", {
        variant: "success",
      });
    },
  });

  if (originalTransaction.isPending) {
    return <p>Loading original transaction...</p>;
  }

  if (originalTransaction.isError) {
    return <p>Error while loading original transaction...</p>;
  }

  return (
    <Box sx={{ p: 2, backgroundColor: "background.paper" }}>
      <Stack spacing={2}>
        <Typography variant="h6">Potenziale Duplicato Rilevato</Typography>
        <Typography variant="body2" color="text.secondary">
          Questa transazione importata è molto simile a una già esistente.
          Controlla i dettagli e decidi come procedere.
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <TransactionCard
              title="Transazione Importata"
              transaction={transaction}
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <TransactionCard
              title="Transazione Esistente"
              transaction={originalTransaction.data}
            />
          </Grid>
        </Grid>
        <Stack direction="row" spacing={2} justifyContent="flex-end">
          <Button
            color="error"
            onClick={() => {
              deleteMutation.mutate();
            }}
            disabled={deleteMutation.isPending}
            variant="text"
          >
            È un duplicato (Elimina)
          </Button>
          <Button
            component={Link}
            to={`/transactions/${transaction.id}`}
            variant="contained"
          >
            Completa
          </Button>
        </Stack>
      </Stack>
    </Box>
  );
}
