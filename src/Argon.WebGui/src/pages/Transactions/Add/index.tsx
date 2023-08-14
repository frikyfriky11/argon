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
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import {
  ITransactionsCreateRequest,
  TransactionsClient,
  TransactionsCreateRequest,
} from "../../../services/backend/BackendClient";
import Form from "./Form";

export default function Add() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [stayAfterSave, setStayAfterSave] = useState(false);

  const mutation = useMutation({
    mutationFn: async (data: ITransactionsCreateRequest) =>
      new TransactionsClient().create(new TransactionsCreateRequest(data)),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });

      enqueueSnackbar(`Transazione ${data.description} creata`, {
        variant: "success",
      });

      if (!stayAfterSave) {
        navigate("/transactions", { replace: true });
      }
    },
  });

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
      <Box width="100%">
        <Card>
          <CardHeader title="Crea nuova transazione" />
          <CardContent>
            <Form
              isSaving={mutation.isLoading}
              onSubmit={(data) => {
                mutation.mutate(data);
              }}
              setStayAfterSave={setStayAfterSave}
              stayAfterSave={stayAfterSave}
            />
          </CardContent>
        </Card>
      </Box>
    </Stack>
  );
}
