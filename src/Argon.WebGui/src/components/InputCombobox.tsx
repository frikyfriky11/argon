import { Autocomplete, AutocompleteProps, TextField } from "@mui/material";
import * as React from "react";
import { Controller, FieldValues, useFormContext } from "react-hook-form";
import { FieldPath, RegisterOptions } from "react-hook-form";

import { resolveJsonPath } from "../utils/ObjectUtils";

export type InputComboboxProps<
  T,
  TFormValues extends FieldValues = FieldValues,
> = {
  field: FieldPath<TFormValues>;
  label: string;
  items: T[];
  itemLabel: (item: T) => string;
  itemValue: (item: T) => unknown;
  options?: RegisterOptions<TFormValues, FieldPath<TFormValues>>;
};

export default function InputCombobox<
  T,
  TFormValues extends FieldValues = FieldValues,
>({
  field,
  label,
  items,
  itemLabel,
  itemValue,
  options,
  ...other
}: InputComboboxProps<T, TFormValues> &
  Omit<AutocompleteProps<T, false, false, false>, "renderInput" | "options">) {
  const { formState, control } = useFormContext<TFormValues>();

  return (
    <Controller
      control={control}
      name={field}
      render={({ field: { onChange, onBlur, value, ref } }) => (
        <Autocomplete
          {...other}
          autoComplete
          autoHighlight
          autoSelect
          getOptionLabel={itemLabel}
          onBlur={onBlur}
          onChange={(_, v) => {
            onChange((v && itemValue(v)) ?? null);
          }}
          options={items}
          ref={ref}
          renderInput={(params) => (
            <TextField
              {...params}
              error={Boolean(resolveJsonPath(field, formState.errors))}
              helperText={
                Boolean(resolveJsonPath(field, formState.errors)) &&
                (
                  resolveJsonPath(field, formState.errors) as {
                    message: string;
                  }
                ).message
              }
              label={label}
            />
          )}
          value={items.find((x) => itemValue(x) === value) ?? null}
        />
      )}
      rules={options}
    />
  );
}
