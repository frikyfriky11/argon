import SaveIcon from "@mui/icons-material/Save";
import { Button, Grid, Stack } from "@mui/material";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import { Link } from "react-router-dom";

import InputText from "../../../components/InputText";
import { ICounterpartyIdentifiersGetResponse } from "../../../services/backend/BackendClient";

export type FormProps = {
  counterpartyIdentifier: ICounterpartyIdentifiersGetResponse;
  onSubmit: SubmitHandler<ICounterpartyIdentifiersGetResponse>;
  isSaving: boolean;
};

export default function Form({
  counterpartyIdentifier,
  onSubmit,
  isSaving,
}: FormProps) {
  const form = useForm<ICounterpartyIdentifiersGetResponse>({
    defaultValues: counterpartyIdentifier,
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
              field={"identifierText"}
              fullWidth
              label="Nome alternativo"
              options={{
                required:
                  "Il nome alternativo della controparte è obbligatorio",
                maxLength: {
                  value: 250,
                  message:
                    "Il nome alternativo della controparte non può superare i 250 caratteri",
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
          <Button
            component={Link}
            to={`/counterparty/${counterpartyIdentifier.counterpartyId}/identifiers`}
            variant="text"
          >
            Annulla
          </Button>
        </Stack>
      </form>
    </FormProvider>
  );
}
