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
import { Link, useNavigate, useParams } from "react-router-dom";

import {
  CounterpartyIdentifiersClient,
  CounterpartyIdentifiersCreateRequest,
  ICounterpartyIdentifiersCreateRequest,
} from "../../../services/backend/BackendClient";
import Form from "./Form";

export default function Add() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const { counterpartyId } = useParams() as { counterpartyId: string };

  const mutation = useMutation({
    mutationFn: async (data: ICounterpartyIdentifiersCreateRequest) =>
      new CounterpartyIdentifiersClient().create(
        new CounterpartyIdentifiersCreateRequest(data),
      ),
    onSuccess: async (_, data) => {
      await queryClient.invalidateQueries({
        queryKey: ["counterpartyIdentifiers"],
      });

      enqueueSnackbar(`Nome alternativo ${data.identifierText} creato`, {
        variant: "success",
      });

      navigate(`/counterparties/${counterpartyId}/identifiers`, {
        replace: true,
      });
    },
  });

  return (
    <Stack alignItems="start" spacing={4}>
      <Button
        color="primary"
        component={Link}
        startIcon={<ArrowBackIcon />}
        to={`/counterparties/${counterpartyId}/identifiers`}
        variant="text"
      >
        Conti
      </Button>
      <Box width="100%">
        <Card>
          <CardHeader title="Crea nuovo nome alternativo" />
          <CardContent>
            <Form
              counterpartyId={counterpartyId ?? ""}
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
