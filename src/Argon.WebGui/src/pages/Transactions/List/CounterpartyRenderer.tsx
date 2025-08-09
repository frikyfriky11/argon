import { Typography } from "@mui/material";

export type CounterpartyRendererProps = {
  effectiveCounterpartyName: string;
  rawImportData: string | null;
};

export default function CounterpartyRenderer({
  effectiveCounterpartyName,
  rawImportData,
}: CounterpartyRendererProps) {
  let parsedCounterpartyName = null;

  if (rawImportData) {
    const parsed = JSON.parse(rawImportData) as { CounterpartyName: string };
    if (parsed.CounterpartyName) {
      parsedCounterpartyName = parsed.CounterpartyName;
    }
  }

  if (parsedCounterpartyName) {
    return (
      <>
        <Typography variant="inherit">{effectiveCounterpartyName}</Typography>
        <Typography variant="inherit" color="textDisabled">
          ({parsedCounterpartyName})
        </Typography>
      </>
    );
  } else {
    return (
      <Typography variant="inherit">{effectiveCounterpartyName}</Typography>
    );
  }
}
