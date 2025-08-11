import { Typography } from "@mui/material";

import { ITransactionRowsGetListResponse } from "../../../services/backend/BackendClient.ts";

export type AccountsRendererProps = {
  rows: ITransactionRowsGetListResponse[];
  rawImportData: string | null;
};

export default function AccountsRenderer({
  rows,
  rawImportData,
}: AccountsRendererProps) {
  let parsedRawDescription = null;

  if (rawImportData) {
    const parsed = JSON.parse(rawImportData) as { RawDescription: string };
    if (parsed.RawDescription) {
      parsedRawDescription = parsed.RawDescription;
    }
  }

  const fromAccounts = rows
    .filter((row) => row.debit !== null)
    .map((row) => row.accountName);
  const toAccounts = rows
    .filter((row) => row.credit !== null)
    .map((row) => row.accountName);

  let accounts = null;

  if (fromAccounts.length > 0 && toAccounts.length > 0) {
    accounts = `${toAccounts.join(" e ")} -> ${fromAccounts.join(" e ")}`;
  }

  if (parsedRawDescription) {
    return (
      <>
        <Typography variant="inherit">{accounts}</Typography>
        <Typography variant="inherit" color="textDisabled">
          ({parsedRawDescription})
        </Typography>
      </>
    );
  } else {
    return <Typography variant="inherit">{accounts}</Typography>;
  }
}
