import { useState } from "react";
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
  BankStatementsClient,
  BankStatementsParseRequest,
  IBankStatementsParseRequest,
} from "../../../services/backend/BackendClient";
import Form from "./Form";
import ParsingWarningsDialog from "./ParsingWarningsDialog";

export default function Add() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const [showWarningsDialog, setShowWarningsDialog] = useState(false);
  const [parsingWarnings, setParsingWarnings] = useState<string[]>([]);

  const mutation = useMutation({
    mutationFn: async (data: IBankStatementsParseRequest) =>
      new BankStatementsClient().parse(new BankStatementsParseRequest(data)),
    onSuccess: async (response, data) => {
      await queryClient.invalidateQueries({ queryKey: ["bankStatements"] });

      enqueueSnackbar(
        `Estratto conto bancario ${data.inputFileName} caricato`,
        {
          variant: "success",
        },
      );

      if (response.warnings && response.warnings.length > 0) {
        setParsingWarnings(response.warnings);
        setShowWarningsDialog(true);
      } else {
        navigate("/bank-statements", { replace: true });
      }
    },
  });

  return (
    <Stack alignItems="start" spacing={4}>
      <Button
        color="primary"
        component={Link}
        startIcon={<ArrowBackIcon />}
        to="/bank-statements"
        variant="text"
      >
        Estratti conto bancari
      </Button>
      <Box width="100%">
        <Card>
          <CardHeader title="Carica nuovo estratto conto bancario" />
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
      <ParsingWarningsDialog
        open={showWarningsDialog}
        onClose={() => {
          setShowWarningsDialog(false);
          navigate("/bank-statements", { replace: true });
        }}
        warnings={parsingWarnings}
      />
    </Stack>
  );
}
