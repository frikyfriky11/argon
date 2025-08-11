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
  CounterpartiesClient,
  CounterpartiesUpdateRequest,
  ICounterpartiesGetResponse,
} from "../../../services/backend/BackendClient";
import ActionMenu from "./ActionMenu";
import Form from "./Form";

export default function Edit() {
  const { id } = useParams() as { id: string };

  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const confirm = useConfirm();

  const counterparty = useQuery({
    queryKey: ["counterparties", id],
    queryFn: () => new CounterpartiesClient().get(id),
  });

  const updateMutation = useMutation({
    mutationFn: async (data: ICounterpartiesGetResponse) =>
      new CounterpartiesClient().update(
        id,
        new CounterpartiesUpdateRequest({
          name: data.name,
        }),
      ),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({ queryKey: ["counterparties"] });

      enqueueSnackbar(`Controparte ${data.name} aggiornata`, {
        variant: "success",
      });

      navigate("/counterparties");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async () => new CounterpartiesClient().delete(id),
    onMutate: async () => {
      await confirm({
        description: (
          <>
            Vuoi davvero eliminare la controparte{" "}
            <strong>{counterparty.data?.name}</strong>?
          </>
        ),
        title: "Elimina controparte",
        confirmationButtonProps: { color: "error", variant: "contained" },
        confirmationText: "Elimina",
        cancellationText: "Annulla",
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["counterparties"],
        refetchType: "none",
      });

      enqueueSnackbar(`Controparte ${counterparty.data?.name} eliminata`, {
        variant: "success",
      });

      navigate("/counterparties", { replace: true });
    },
  });

  if (counterparty.isPending) {
    return <p>Loading counterparty...</p>;
  }

  if (counterparty.isError) {
    return <p>Error while loading counterparty...</p>;
  }

  return (
    <Stack alignItems="start" spacing={4}>
      <Button
        color="primary"
        component={Link}
        startIcon={<ArrowBackIcon />}
        to="/counterparties"
        variant="text"
      >
        Conti
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
            <Typography variant="h5">{counterparty.data.name}</Typography>
            <Typography variant="caption">
              ID: {counterparty.data.id}
            </Typography>
          </Stack>
        </Stack>
        <Stack direction="row" spacing={2}>
          <ActionMenu onDelete={deleteMutation.mutate} />
        </Stack>
      </Stack>
      <Box width="100%">
        <Card>
          <CardHeader title="Modifica controparte" />
          <CardContent>
            <Form
              counterparty={counterparty.data}
              isSaving={updateMutation.isPending}
              onSubmit={(data) => {
                updateMutation.mutate(data);
              }}
            />
          </CardContent>
        </Card>
      </Box>
    </Stack>
  );
}
