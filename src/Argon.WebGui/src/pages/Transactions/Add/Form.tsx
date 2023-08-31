import AddCircleIcon from "@mui/icons-material/AddCircle";
import DragIndicatorIcon from "@mui/icons-material/DragIndicator";
import RemoveCircleIcon from "@mui/icons-material/RemoveCircle";
import SaveIcon from "@mui/icons-material/Save";
import {
  Box,
  Button,
  FormControlLabel,
  Grid,
  IconButton,
  Stack,
  Switch,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import Decimal from "decimal.js";
import { DateTime } from "luxon";
import { useEffect, useRef } from "react";
import {
  FormProvider,
  SubmitHandler,
  useFieldArray,
  useForm,
} from "react-hook-form";
import { Link } from "react-router-dom";

import InputCombobox from "../../../components/InputCombobox";
import InputCurrency from "../../../components/InputCurrency";
import InputDate from "../../../components/InputDate";
import InputText from "../../../components/InputText";
import {
  AccountsClient,
  ITransactionsCreateRequest,
  TransactionRowsCreateRequest,
} from "../../../services/backend/BackendClient";

export type FormProps = {
  onSubmit: SubmitHandler<ITransactionsCreateRequest>;
  isSaving: boolean;
  stayAfterSave: boolean;
  setStayAfterSave: (value: boolean) => void;
};

export default function Form({
  onSubmit,
  isSaving,
  stayAfterSave,
  setStayAfterSave,
}: FormProps) {
  const accounts = useQuery({
    queryKey: ["accounts"],
    queryFn: () => new AccountsClient().getList(undefined),
  });

  const dateInputRef = useRef<HTMLDivElement | null>(null);

  const form = useForm<ITransactionsCreateRequest>({
    defaultValues: {
      date: DateTime.now(),
      description: "Nuova transazione 1",
      transactionRows: [
        new TransactionRowsCreateRequest({
          rowCounter: 1,
          credit: null,
          debit: null,
          // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
          accountId: null!,
          description: "",
        }),
        new TransactionRowsCreateRequest({
          rowCounter: 2,
          credit: null,
          debit: null,
          // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
          accountId: null!,
          description: "",
        }),
      ],
    },
  });

  const { fields, append, remove, update } = useFieldArray({
    control: form.control,
    name: "transactionRows",
  });

  const formValues = form.watch();

  useEffect(() => {
    const transactionRows = formValues.transactionRows;

    const debitSum = transactionRows.reduce(
      (acc, row) => acc.plus(new Decimal(row.debit ?? 0)),
      new Decimal(0),
    );
    const creditSum = transactionRows.reduce(
      (acc, row) => acc.plus(new Decimal(row.credit ?? 0)),
      new Decimal(0),
    );

    const missing = debitSum.minus(creditSum);

    if (!missing.isZero()) {
      const firstEmptyRowIndex = transactionRows.findIndex(
        (row) => row.debit === null && row.credit === null,
      );

      if (firstEmptyRowIndex != -1) {
        if (missing.isNegative()) {
          update(
            firstEmptyRowIndex,
            new TransactionRowsCreateRequest({
              ...transactionRows[firstEmptyRowIndex],
              debit: Math.abs(missing.toNumber()),
            }),
          );
        }
        if (missing.isPositive()) {
          update(
            firstEmptyRowIndex,
            new TransactionRowsCreateRequest({
              ...transactionRows[firstEmptyRowIndex],
              credit: Math.abs(missing.toNumber()),
            }),
          );
        }
      }
    }
  }, [formValues, update]);

  if (accounts.isLoading) {
    return <p>Loading accounts...</p>;
  }

  if (accounts.isError) {
    return <p>Error while loading accounts...</p>;
  }

  return (
    <FormProvider {...form}>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          void form.handleSubmit((data) => {
            onSubmit(data);
            form.reset({
              date: formValues.date,
              description: "",
            });
            // Clear the field array
            remove();
            // Add initial fields here
            const initialFields = [
              new TransactionRowsCreateRequest({
                rowCounter: 1,
                credit: null,
                debit: null,
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                accountId: null!,
                description: "",
              }),
              new TransactionRowsCreateRequest({
                rowCounter: 2,
                credit: null,
                debit: null,
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                accountId: null!,
                description: "",
              }),
            ];
            initialFields.forEach((row) => {
              append(row);
            });

            // focus the date input to be ready to insert new data immediately
            if (dateInputRef.current) {
              dateInputRef.current.focus();
            }
          })(e);
        }}
      >
        <Stack gap={4}>
          <Grid container spacing={2}>
            <Grid item sm={3} xs={12}>
              <InputDate
                field={"date"}
                fullWidth
                inputRef={dateInputRef}
                label="Data"
                options={{ required: "La data è obbligatoria" }}
              />
            </Grid>
            <Grid item sm={9} xs={12}>
              <InputText
                field={"description"}
                fullWidth
                label="Descrizione"
                options={{
                  required: "La descrizione della transazione è obbligatoria",
                  maxLength: {
                    value: 100,
                    message:
                      "La descrizione della transazione non può superare i 50 caratteri",
                  },
                }}
              />
            </Grid>
            {fields.map((field, index) => (
              <Grid item key={field.id} xs={12}>
                <Stack alignItems="center" direction="row" gap={1}>
                  <Box>
                    <IconButton tabIndex={-1}>
                      <DragIndicatorIcon />
                    </IconButton>
                  </Box>
                  <Grid container item spacing={2}>
                    <Grid item sm={5} xs={12}>
                      <InputCombobox
                        field={`transactionRows.${index}.accountId`}
                        fullWidth
                        itemLabel={(item) => item.name}
                        itemValue={(item) => item.id}
                        items={accounts.data}
                        label={"Conto"}
                        options={{ required: "Il conto è obbligatorio" }}
                      />
                    </Grid>
                    <Grid item sm={3} xs={12}>
                      <InputText
                        field={`transactionRows.${index}.description`}
                        fullWidth
                        label="Descrizione"
                        options={{
                          maxLength: {
                            value: 100,
                            message:
                              "La descrizione della riga non può superare i 100 caratteri",
                          },
                        }}
                      />
                    </Grid>
                    <Grid item sm={2} xs={2}>
                      <InputCurrency
                        field={`transactionRows.${index}.debit`}
                        fullWidth
                        label={"Dare"}
                      />
                    </Grid>
                    <Grid item sm={2} xs={2}>
                      <InputCurrency
                        field={`transactionRows.${index}.credit`}
                        fullWidth
                        label={"Avere"}
                      />
                    </Grid>
                  </Grid>
                  <Box>
                    <IconButton
                      onClick={() => {
                        remove(index);
                      }}
                      tabIndex={-1}
                    >
                      <RemoveCircleIcon />
                    </IconButton>
                  </Box>
                </Stack>
              </Grid>
            ))}
            <Grid item xs={12}>
              <Box>
                <IconButton
                  onClick={() => {
                    append(
                      new TransactionRowsCreateRequest({
                        rowCounter: formValues.transactionRows.length + 1,
                        credit: null,
                        debit: null,
                        description: "",
                        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                        accountId: null!,
                      }),
                    );
                  }}
                  tabIndex={-1}
                >
                  <AddCircleIcon />
                </IconButton>
              </Box>
            </Grid>
          </Grid>

          <FormControlLabel
            control={
              <Switch
                onChange={(e) => {
                  setStayAfterSave(e.target.checked);
                }}
                value={stayAfterSave}
              />
            }
            label="Rimani su questa pagina dopo aver salvato"
          />

          <Stack direction="row" spacing={2}>
            <Button
              disabled={isSaving}
              startIcon={<SaveIcon />}
              type="submit"
              variant="contained"
            >
              Salva
            </Button>
            <Button component={Link} to="/transactions" variant="text">
              Annulla
            </Button>
          </Stack>
        </Stack>
      </form>
    </FormProvider>
  );
}
