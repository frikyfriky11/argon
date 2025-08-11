import { TextField, TextFieldProps } from "@mui/material";
import { FieldValues, useFormContext } from "react-hook-form";
import { FieldPath, RegisterOptions } from "react-hook-form";

import { resolveJsonPath } from "../utils/ObjectUtils";

export type InputTextProps<TFormValues extends FieldValues = FieldValues> = {
  field: FieldPath<TFormValues>;
  label: string;
  options?: RegisterOptions<TFormValues, FieldPath<TFormValues>>;
};

export default function InputText<
  TFieldValues extends FieldValues = FieldValues,
>({
  field,
  label,
  options,
  ...other
}: InputTextProps<TFieldValues> & TextFieldProps) {
  const { formState, register } = useFormContext<TFieldValues>();

  return (
    <TextField
      {...other}
      label={label}
      {...register(field, options)}
      error={Boolean(resolveJsonPath(field, formState.errors))}
      helperText={
        Boolean(resolveJsonPath(field, formState.errors)) &&
        (resolveJsonPath(field, formState.errors) as { message: string })
          .message
      }
    />
  );
}
