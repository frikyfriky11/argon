import { TextFieldProps } from "@mui/material";
import { DatePicker } from "@mui/x-date-pickers";
import * as React from "react";
import { Controller, FieldValues, useFormContext } from "react-hook-form";
import { FieldPath } from "react-hook-form/dist/types/path";
import { RegisterOptions } from "react-hook-form/dist/types/validator";

import { resolveJsonPath } from "../utils/ObjectUtils";

export type InputDateProps<TFormValues extends FieldValues = FieldValues> = {
  field: FieldPath<TFormValues>;
  label: string;
  options?: RegisterOptions<TFormValues, FieldPath<TFormValues>>;
};

export default function InputDate<
  TFormValues extends FieldValues = FieldValues,
>({
  field,
  label,
  options,
  ...other
}: InputDateProps<TFormValues> & TextFieldProps) {
  const { formState, control } = useFormContext<TFormValues>();

  return (
    <Controller
      control={control}
      name={field}
      render={({ field: { onChange, onBlur, value, ref } }) => (
        <DatePicker
          onChange={onChange}
          ref={ref}
          slotProps={{
            textField: {
              ...other,
              onBlur,
              label,
              error: Boolean(resolveJsonPath(field, formState.errors)),
              helperText:
                Boolean(resolveJsonPath(field, formState.errors)) &&
                (
                  resolveJsonPath(field, formState.errors) as {
                    message: string;
                  }
                ).message,
            },
          }}
          value={value}
        />
      )}
      rules={options}
    />
  );
}
