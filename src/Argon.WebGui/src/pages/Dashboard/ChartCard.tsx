import {
  Box,
  Card,
  CardContent,
  CardHeader,
  CircularProgress,
  Typography,
} from "@mui/material";
import React from "react";

export type ChartCardProps = {
  title: string;
  subtitle?: string;
  isPending: boolean;
  isError: boolean;
  isEmpty: boolean;
  height?: number;
  children: React.ReactNode;
};

/**
 * Wraps a dashboard chart in a titled card and renders the loading / error /
 * empty states consistently so each chart component only has to draw its data.
 */
export default function ChartCard({
  title,
  subtitle,
  isPending,
  isError,
  isEmpty,
  height = 300,
  children,
}: ChartCardProps) {
  return (
    <Card>
      <CardHeader subheader={subtitle} title={title} />
      <CardContent>
        <Box
          alignItems="center"
          display="flex"
          justifyContent="center"
          minHeight={height}
        >
          {isPending ? (
            <CircularProgress />
          ) : isError ? (
            <Typography color="error">
              Errore durante il caricamento dei dati.
            </Typography>
          ) : isEmpty ? (
            <Typography color="text.secondary">
              Nessun dato per il periodo selezionato.
            </Typography>
          ) : (
            <Box width="100%">{children}</Box>
          )}
        </Box>
      </CardContent>
    </Card>
  );
}
