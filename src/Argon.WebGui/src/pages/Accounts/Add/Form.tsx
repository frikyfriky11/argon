import SaveIcon from "@mui/icons-material/Save";
import { Button, Grid, Stack } from "@mui/material";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import { Link } from "react-router-dom";

import InputCombobox from "../../../components/InputCombobox";
import InputText from "../../../components/InputText";
import AccountTypeConverter from "../../../enums/AccountTypeConverter";
import { getEnumAsArray } from "../../../enums/EnumHelpers";
import {
  AccountType,
  IAccountsCreateRequest,
} from "../../../services/backend/BackendClient";

export type FormProps = {
  onSubmit: SubmitHandler<IAccountsCreateRequest>;
  isSaving: boolean;
};

export default function Form({ onSubmit, isSaving }: FormProps) {
  const form = useForm<IAccountsCreateRequest>({
    defaultValues: {
      name: "Nuovo conto 1",
      type: AccountType.Expense,
    },
  });

  return (
    <FormProvider {...form}>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          void form.handleSubmit(onSubmit)(e);
        }}
      >
        <Grid container spacing={2}>
          <Grid item sm={6} xs={12}>
            <InputText
              field={"name"}
              fullWidth
              label="Nome"
              options={{
                required: "Il nome del conto è obbligatorio",
                maxLength: {
                  value: 50,
                  message: "Il nome del conto non può superare i 50 caratteri",
                },
              }}
            />
          </Grid>
          <Grid item sm={6} xs={12}>
            <InputCombobox
              field={"type"}
              fullWidth
              itemLabel={(item) => item.text}
              itemValue={(item) => item.value}
              items={getEnumAsArray(AccountType, AccountTypeConverter.convert)}
              label={"Tipo"}
              options={{ required: "Il tipo di conto è obbligatorio" }}
            />
          </Grid>
        </Grid>
        <Stack direction="row" spacing={2} sx={{ mt: 4 }}>
          <Button
            disabled={isSaving}
            startIcon={<SaveIcon />}
            type="submit"
            variant="contained"
          >
            Salva
          </Button>
          <Button component={Link} to="/accounts" variant="text">
            Annulla
          </Button>
        </Stack>
      </form>
    </FormProvider>
  );
}
