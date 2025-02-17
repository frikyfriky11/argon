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
  CounterpartiesClient,
  CounterpartiesCreateRequest,
  ICounterpartiesCreateRequest,
} from "../../../services/backend/BackendClient";
import Form from "./Form";

export default function Add() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: async (data: ICounterpartiesCreateRequest) =>
      new CounterpartiesClient().create(new CounterpartiesCreateRequest(data)),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({ queryKey: ["counterparties"] });

      enqueueSnackbar(`Controparte ${data.name} creata`, {
        variant: "success",
      });

      navigate("/counterparties", { replace: true });
    },
  });

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
      <Box width="100%">
        <Card>
          <CardHeader title="Crea nuova controparte" />
          <CardContent>
            <Form
              isSaving={mutation.isPending}
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
