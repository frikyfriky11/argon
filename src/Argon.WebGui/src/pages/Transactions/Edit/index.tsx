import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import FolderIcon from "@mui/icons-material/Folder";
import {
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Stack,
  Typography,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useConfirm } from "material-ui-confirm";
import { enqueueSnackbar } from "notistack";
import { Link, useNavigate, useParams } from "react-router-dom";

import {
  ITransactionsGetResponse,
  TransactionRowsUpdateRequest,
  TransactionsClient,
  TransactionsUpdateRequest,
} from "../../../services/backend/BackendClient";
import ActionMenu from "./ActionMenu";
import Form from "./Form";

export default function Edit() {
  const { id } = useParams() as { id: string };

  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const confirm = useConfirm();

  const transaction = useQuery({
    queryKey: ["transactions", id],
    queryFn: () =>
      new TransactionsClient().get(id).then((data) => {
        data.transactionRows.sort((a, b) => a.rowCounter - b.rowCounter);
        return data;
      }),
  });

  const updateMutation = useMutation({
    mutationFn: async (data: ITransactionsGetResponse) =>
      new TransactionsClient().update(
        id,
        new TransactionsUpdateRequest({
          date: data.date,
          description: data.description,
          transactionRows: data.transactionRows.map(
            (row) =>
              new TransactionRowsUpdateRequest({
                rowCounter: row.rowCounter,
                credit: row.credit,
                debit: row.debit,
                description: row.description,
                accountId: row.accountId,
                id: row.id,
              }),
          ),
        }),
      ),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });

      enqueueSnackbar(`Transazione ${data.description} aggiornata`, {
        variant: "success",
      });

      navigate("/transactions");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async () => new TransactionsClient().delete(id),
    onMutate: async () => {
      await confirm({
        description: (
          <>
            Vuoi davvero eliminare la transazione{" "}
            <strong>{transaction.data?.description}</strong>?
          </>
        ),
        title: "Elimina transazione",
        confirmationButtonProps: { color: "error", variant: "contained" },
        confirmationText: "Elimina",
        cancellationText: "Annulla",
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["transactions"],
        refetchType: "none",
      });

      enqueueSnackbar(
        `Transazione ${transaction.data?.description} eliminata`,
        {
          variant: "success",
        },
      );

      navigate("/transactions", { replace: true });
    },
  });

  if (transaction.isPending) {
    return <p>Loading transaction...</p>;
  }

  if (transaction.isError) {
    return <p>Error while loading transaction...</p>;
  }

  return (
    <Stack alignItems="start" spacing={4}>
      <Button
        color="primary"
        component={Link}
        startIcon={<ArrowBackIcon />}
        to="/transactions"
        variant="text"
      >
        Transazioni
      </Button>
      <Stack
        alignItems="center"
        direction="row"
        justifyContent="space-between"
        spacing={2}
        sx={{ width: "100%" }}
      >
        <Stack direction="row" spacing={2}>
          <Avatar sx={{ width: 64, height: 64 }}>
            <FolderIcon />
          </Avatar>
          <Stack justifyContent="space-between">
            <Typography variant="h5">{transaction.data.description}</Typography>
            <Typography variant="caption">ID: {transaction.data.id}</Typography>
          </Stack>
        </Stack>
        <Stack direction="row" spacing={2}>
          <ActionMenu onDelete={deleteMutation.mutate} />
        </Stack>
      </Stack>
      <Box width="100%">
        <Card>
          <CardHeader title="Modifica transazione" />
          <CardContent>
            <Form
              isSaving={updateMutation.isPending}
              onSubmit={(data) => {
                updateMutation.mutate(data);
              }}
              transaction={transaction.data}
            />
          </CardContent>
        </Card>
      </Box>
    </Stack>
  );
}
