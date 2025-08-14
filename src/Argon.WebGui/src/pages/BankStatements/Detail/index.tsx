import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteIcon from "@mui/icons-material/Delete";
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Stack,
} from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useConfirm } from "material-ui-confirm";
import { enqueueSnackbar } from "notistack";
import { Link, useNavigate, useParams } from "react-router-dom";

import { BankStatementsClient } from "../../../services/backend/BackendClient";
import Results from "./Results";
import Toolbar from "./Toolbar";

export default function Index() {
  const { id } = useParams() as { id: string };

  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const confirm = useConfirm();

  const bankStatement = useQuery({
    queryKey: ["bankStatements", id],
    queryFn: () => new BankStatementsClient().get(id),
  });

  const deleteMutation = useMutation({
    mutationFn: async () => new BankStatementsClient().delete(id),
    onMutate: async () => {
      await confirm({
        description: (
          <>
            Vuoi davvero eliminare l'estratto conto{" "}
            <strong>{bankStatement.data?.fileName}</strong> e tutte le
            transazioni importate da esso?
          </>
        ),
        title: "Elimina estratto conto",
        confirmationButtonProps: { color: "error", variant: "contained" },
        confirmationText: "Elimina",
        cancellationText: "Annulla",
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["bankStatements"],
        refetchType: "none",
      });

      enqueueSnackbar(
        `Estratto conto ${bankStatement.data?.fileName} eliminato`,
        {
          variant: "success",
        },
      );

      navigate("/bank-statements", { replace: true });
    },
  });

  if (bankStatement.isPending) {
    return <p>Loading bank statement...</p>;
  }

  if (bankStatement.isError) {
    return <p>Error while loading bank statement...</p>;
  }

  return (
    <Stack alignItems="start" spacing={4}>
      <Stack direction="row" justifyContent="space-between" width="100%">
        <Button
          color="primary"
          component={Link}
          startIcon={<ArrowBackIcon />}
          to="/bank-statements"
          variant="text"
        >
          Estratti conto bancari
        </Button>
        <Button
          color="error"
          startIcon={<DeleteIcon />}
          onClick={() => {
            deleteMutation.mutate();
          }}
          disabled={deleteMutation.isPending}
          variant="contained"
        >
          Annulla Importazione
        </Button>
      </Stack>
      <Toolbar fileName={bankStatement.data.fileName} />
      <Box width="100%">
        <Card>
          <CardHeader title="Transazioni importate" />
          <CardContent>
            <Results transactions={bankStatement.data.transactions} />
          </CardContent>
        </Card>
      </Box>
    </Stack>
  );
}
