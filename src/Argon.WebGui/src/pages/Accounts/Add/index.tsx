import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Stack,
} from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { enqueueSnackbar } from "notistack";
import { Link, useNavigate } from "react-router-dom";

import {
  AccountsClient,
  AccountsCreateRequest,
  IAccountsCreateRequest,
} from "../../../services/backend/BackendClient";
import Form from "./Form";

export default function Add() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: async (data: IAccountsCreateRequest) =>
      new AccountsClient().create(new AccountsCreateRequest(data)),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });

      enqueueSnackbar(`Conto ${data.name} creato`, {
        variant: "success",
      });

      navigate("/accounts", { replace: true });
    },
  });

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
      <Box width="100%">
        <Card>
          <CardHeader title="Crea nuovo conto" />
          <CardContent>
            <Form
              isSaving={mutation.isLoading}
              onSubmit={(data) => {
                mutation.mutate(data);
              }}
            />
          </CardContent>
        </Card>
      </Box>
    </Stack>
  );
}
