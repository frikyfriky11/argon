import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Stack,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { Link, useParams } from "react-router-dom";

import { BankStatementsClient } from "../../../services/backend/BackendClient";
import Results from "./Results";
import Toolbar from "./Toolbar";

export default function Index() {
  const { id } = useParams() as { id: string };

  const bankStatement = useQuery({
    queryKey: ["bankStatements", id],
    queryFn: () => new BankStatementsClient().get(id),
  });

  if (bankStatement.isPending) {
    return <p>Loading bank statement...</p>;
  }

  if (bankStatement.isError) {
    return <p>Error while loading bank statement...</p>;
  }

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
