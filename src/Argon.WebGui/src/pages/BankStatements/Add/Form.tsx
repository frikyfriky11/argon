import SaveIcon from "@mui/icons-material/Save";
import { Button, Grid, Stack } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { MuiFileInput } from "mui-file-input";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import { Link } from "react-router-dom";

import InputCombobox from "../../../components/InputCombobox";
import {
  AccountsClient,
  BankStatementParsersGetListRequest,
  BankStatementsClient,
  IBankStatementsParseRequest,
} from "../../../services/backend/BackendClient";

export type FormProps = {
  onSubmit: SubmitHandler<IBankStatementsParseRequest>;
  isSaving: boolean;
};

export default function Form({ onSubmit, isSaving }: FormProps) {
  const accounts = useQuery({
    queryKey: ["accounts"],
    queryFn: () => new AccountsClient().getList(null, null),
  });
  const parsers = useQuery({
    queryKey: ["parsers"],
    queryFn: () =>
      new BankStatementsClient().parsersGetList(
        new BankStatementParsersGetListRequest(),
      ),
  });

  const form = useForm<IBankStatementsParseRequest>({
    defaultValues: {
      inputFileName: "",
      inputFileContents: "",
      importToAccountId: null!,
      parserId: null!,
    },
  });

  if (accounts.isPending) {
    return <p>Loading accounts...</p>;
  }

  if (accounts.isError) {
    return <p>Error while loading accounts...</p>;
  }

  if (parsers.isPending) {
    return <p>Loading parsers...</p>;
  }

  if (parsers.isError) {
    return <p>Error while loading parsers...</p>;
  }

  const fileToBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      // Set up the load event handler
      reader.onload = () => {
        // The result property contains the data as a URL representing the file's contents as a Base64 encoded string
        // It will look like "data:image/png;base64,iVBORw0KGgoAAAANSUhEU..."
        const base64String = reader.result as string;
        resolve(base64String);
      };

      // Set up the error event handler
      reader.onerror = (error) => {
        reject(error);
      };

      // Read the file as a Data URL (base64 encoded string)
      reader.readAsDataURL(file);
    });
  };

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
            <MuiFileInput
              onChange={(file) => {
                if (file) {
                  form.setValue("inputFileName", file.name);
                  void fileToBase64(file).then((base64String) => {
                    form.setValue(
                      "inputFileContents",
                      base64String.split(",")[1],
                    );
                  });
                } else {
                  form.setValue("inputFileName", null!);
                  form.setValue("inputFileContents", null!);
                }
              }}
            />
            {/*<InputText*/}
            {/*  field={"inputFileName"}*/}
            {/*  fullWidth*/}
            {/*  label="Nome"*/}
            {/*  options={{*/}
            {/*    required: "Il nome del conto è obbligatorio",*/}
            {/*    maxLength: {*/}
            {/*      value: 50,*/}
            {/*      message: "Il nome del conto non può superare i 50 caratteri",*/}
            {/*    },*/}
            {/*  }}*/}
            {/*/>*/}
          </Grid>
          <Grid item sm={6} xs={12}>
            <InputCombobox
              field={"parserId"}
              fullWidth
              itemLabel={(item) => item.parserDisplayName}
              itemValue={(item) => item.parserId}
              items={parsers.data}
              label={"Parser"}
              options={{ required: "Il parser è obbligatorio" }}
            />
          </Grid>
          <Grid item sm={6} xs={12}>
            <InputCombobox
              field={"importToAccountId"}
              fullWidth
              itemLabel={(item) => item.name}
              itemValue={(item) => item.id}
              items={accounts.data}
              label={"Conto di riferimento"}
              options={{ required: "Il conto di riferimento è obbligatorio" }}
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
            Carica
          </Button>
          <Button component={Link} to="/bank-statements" variant="text">
            Annulla
          </Button>
        </Stack>
      </form>
    </FormProvider>
  );
}
