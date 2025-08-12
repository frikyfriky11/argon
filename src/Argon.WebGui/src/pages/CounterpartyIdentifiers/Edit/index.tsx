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
  CounterpartyIdentifiersClient,
  CounterpartyIdentifiersUpdateRequest,
  ICounterpartyIdentifiersGetResponse,
} from "../../../services/backend/BackendClient";
import ActionMenu from "./ActionMenu";
import Form from "./Form";

export default function Edit() {
  const { id, counterpartyId } = useParams() as {
    id: string;
    counterpartyId: string;
  };

  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const confirm = useConfirm();

  const counterpartyIdentifier = useQuery({
    queryKey: ["counterpartyIdentifiers", id],
    queryFn: () => new CounterpartyIdentifiersClient().get(id),
  });

  const updateMutation = useMutation({
    mutationFn: async (data: ICounterpartyIdentifiersGetResponse) =>
      new CounterpartyIdentifiersClient().update(
        id,
        new CounterpartyIdentifiersUpdateRequest({
          counterpartyId: data.counterpartyId,
          identifierText: data.identifierText,
        }),
      ),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({
        queryKey: ["counterpartyIdentifiers"],
      });

      enqueueSnackbar(`Nome alternativo ${data.identifierText} aggiornato`, {
        variant: "success",
      });

      navigate(`/counterparties/${counterpartyId}/identifiers`);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async () => new CounterpartyIdentifiersClient().delete(id),
    onMutate: async () => {
      await confirm({
        description: (
          <>
            Vuoi davvero eliminare il nome alternativo{" "}
            <strong>{counterpartyIdentifier.data?.identifierText}</strong>?
          </>
        ),
        title: "Elimina nome alternativo",
        confirmationButtonProps: { color: "error", variant: "contained" },
        confirmationText: "Elimina",
        cancellationText: "Annulla",
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["counterpartyIdentifiers"],
        refetchType: "none",
      });

      enqueueSnackbar(
        `Nome alternativo ${counterpartyIdentifier.data?.identifierText} eliminato`,
        {
          variant: "success",
        },
      );

      navigate(`/counterparties/${counterpartyId}/identifiers`, {
        replace: true,
      });
    },
  });

  if (counterpartyIdentifier.isPending) {
    return <p>Loading counterparty identifier...</p>;
  }

  if (counterpartyIdentifier.isError) {
    return <p>Error while loading counterparty identifier...</p>;
  }

  return (
    <Stack alignItems="start" spacing={4}>
      <Button
        color="primary"
        component={Link}
        startIcon={<ArrowBackIcon />}
        to={`/counterparty/${counterpartyId}/identifiers`}
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
            <Typography variant="h5">
              {counterpartyIdentifier.data.identifierText}
            </Typography>
            <Typography variant="caption">
              ID: {counterpartyIdentifier.data.id}
            </Typography>
          </Stack>
        </Stack>
        <Stack direction="row" spacing={2}>
          <ActionMenu onDelete={deleteMutation.mutate} />
        </Stack>
      </Stack>
      <Box width="100%">
        <Card>
          <CardHeader title="Modifica nome alternativo" />
          <CardContent>
            <Form
              counterpartyIdentifier={counterpartyIdentifier.data}
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
