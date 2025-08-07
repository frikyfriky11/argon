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
  AccountsClient,
  AccountsFavouriteRequest,
  AccountsUpdateRequest,
  IAccountsGetResponse,
} from "../../../services/backend/BackendClient";
import ActionMenu from "./ActionMenu";
import Form from "./Form";

export default function Edit() {
  const { id } = useParams() as { id: string };

  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const confirm = useConfirm();

  const account = useQuery({
    queryKey: ["accounts", id],
    queryFn: () => new AccountsClient().get(id),
  });

  const updateMutation = useMutation({
    mutationFn: async (data: IAccountsGetResponse) =>
      new AccountsClient().update(
        id,
        new AccountsUpdateRequest({
          name: data.name,
          type: data.type,
        }),
      ),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });

      enqueueSnackbar(`Conto ${data.name} aggiornato`, { variant: "success" });

      navigate("/accounts");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: async () => new AccountsClient().delete(id),
    onMutate: async () => {
      await confirm({
        description: (
          <>
            Vuoi davvero eliminare il conto{" "}
            <strong>{account.data?.name}</strong>?
          </>
        ),
        title: "Elimina conto",
        confirmationButtonProps: { color: "error", variant: "contained" },
        confirmationText: "Elimina",
        cancellationText: "Annulla",
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["accounts"],
        refetchType: "none",
      });

      enqueueSnackbar(`Conto ${account.data?.name} eliminato`, {
        variant: "success",
      });

      navigate("/accounts", { replace: true });
    },
  });

  const favouriteMutation = useMutation({
    mutationFn: async () =>
      new AccountsClient().favourite(
        id,
        new AccountsFavouriteRequest({
          isFavourite: !account.data?.isFavourite,
        }),
      ),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["accounts"],
      });

      enqueueSnackbar(
        `Conto ${account.data?.name} ${
          account.data?.isFavourite ? "rimosso dai" : "aggiunto ai"
        } preferiti`,
        {
          variant: "success",
        },
      );
    },
  });

  if (account.isPending) {
    return <p>Loading account...</p>;
  }

  if (account.isError) {
    return <p>Error while loading account...</p>;
  }

  return (
    <Stack alignItems="start" spacing={4}>
      <Button
        color="primary"
        component={Link}
        startIcon={<ArrowBackIcon />}
        to="/accounts"
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
            <Typography variant="h5">{account.data.name}</Typography>
            <Typography variant="caption">ID: {account.data.id}</Typography>
          </Stack>
        </Stack>
        <Stack direction="row" spacing={2}>
          <ActionMenu
            isFavourite={account.data.isFavourite}
            onDelete={deleteMutation.mutate}
            onFavourite={favouriteMutation.mutate}
          />
        </Stack>
      </Stack>
      <Box width="100%">
        <Card>
          <CardHeader title="Modifica conto" />
          <CardContent>
            <Form
              account={account.data}
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
