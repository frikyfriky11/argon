import SaveIcon from "@mui/icons-material/Save";
import { Button, Grid, Stack } from "@mui/material";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import { Link } from "react-router-dom";

import InputText from "../../../components/InputText";
import { ICounterpartiesGetResponse } from "../../../services/backend/BackendClient";

export type FormProps = {
  counterparty: ICounterpartiesGetResponse;
  onSubmit: SubmitHandler<ICounterpartiesGetResponse>;
  isSaving: boolean;
};

export default function Form({ counterparty, onSubmit, isSaving }: FormProps) {
  const form = useForm<ICounterpartiesGetResponse>({
    defaultValues: counterparty,
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
                required: "Il nome della controparte è obbligatorio",
                maxLength: {
                  value: 100,
                  message:
                    "Il nome della controparte non può superare i 100 caratteri",
                },
              }}
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
          <Button component={Link} to="/counterparties" variant="text">
            Annulla
          </Button>
        </Stack>
      </form>
    </FormProvider>
  );
}
