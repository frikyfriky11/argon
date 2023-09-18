import { TextField, TextFieldProps } from "@mui/material";
import React, { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

type InputCurrencyMiniProps = {
  fieldValue?: number | null;
  onFieldBlur: (value: number | null) => void;
};

export default function InputCurrencyMini({
  fieldValue,
  onFieldBlur,
  ...other
}: InputCurrencyMiniProps & TextFieldProps) {
  const { i18n } = useTranslation();

  const numberFormat = useMemo(
    () =>
      new Intl.NumberFormat(i18n.language, {
        style: "decimal",
        minimumFractionDigits: 2,
      }),
    [i18n.language],
  );

  /**
   * To find out which decimal separator is set for the user's locale, we can
   * try to format a number such as 1.1 and find the decimal separator from it.
   * if we can't detect anything for some reason (should never happen), we can
   * gracefully fall back to a comma
   */
  const decimalSeparator = useMemo(
    () =>
      numberFormat.formatToParts(1.1).find((part) => part.type === "decimal")
        ?.value ?? ",",
    [numberFormat],
  );

  const [isEditing, setIsEditing] = useState(false);

  const [rawValue, setRawValue] = useState<string>(
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument,@typescript-eslint/no-unsafe-call
    fieldValue?.toString() ?? "",
  );

  useEffect(() => {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-argument,@typescript-eslint/no-unsafe-call
    setRawValue(fieldValue?.toString() ?? "");
  }, [fieldValue]);

  const floatValue = useMemo(
    () => parseFloat(rawValue.replaceAll(decimalSeparator, ".")),
    [decimalSeparator, rawValue],
  );

  const formattedValue = useMemo(
    () => (Number.isNaN(floatValue) ? "" : numberFormat.format(floatValue)),
    [floatValue, numberFormat],
  );

  const handleFocus = (event: React.FocusEvent<HTMLInputElement>) => {
    if (Number.isNaN(floatValue)) {
      setRawValue("");
    } else {
      setRawValue(
        new Intl.NumberFormat(i18n.language, {
          useGrouping: false,
          style: "decimal",
        }).format(floatValue),
      );
    }
    setTimeout(() => {
      event.target.select();
    }, 0);
    setIsEditing(true);
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    // we can exit early if the user is trying to clear the field (input is "")
    if (!event.target.value) {
      console.log("User is clearing the input");
      setRawValue("");
      return;
    }

    // first, convert all commas into dots to make it easier to spot unwanted chars
    const dottedInput = event.target.value.replaceAll(",", ".");

    // check if the user input has more than one decimal separator,
    // for example, if the user is trying to insert 1234..56 or 1234,,56 or 1234.,56
    if ((dottedInput.match(/\./gi) ?? []).length > 1) {
      console.warn(
        "More than one decimal separator was found in the string",
        event.target.value,
      );
      return;
    }

    // at this point, we should have a string with only one dot, for example, 1234.56
    // check if the user input has only digits (ignoring the dot)
    if (!/^\d+$/.test(dottedInput.replace(".", ""))) {
      console.warn(
        "Characters other than digits and decimal separator were found in the string",
        event.target.value,
      );
      return;
    }

    setRawValue(dottedInput.replaceAll(".", decimalSeparator));
  };

  const handleBlur = () => {
    setIsEditing(false);

    if (Number.isNaN(floatValue)) {
      onFieldBlur(null);
    } else {
      onFieldBlur(floatValue);
    }
  };

  const value = isEditing ? rawValue : formattedValue;

  return (
    <TextField
      variant="outlined"
      {...other}
      inputMode="decimal"
      inputProps={{
        sx: {
          fontFamily: "monospace",
          textAlign: "right",
          fontSize: "0.875rem",
        },
      }}
      onBlur={handleBlur}
      onChange={handleChange}
      onFocus={handleFocus}
      type="text"
      value={value || ""}
    />
  );
}
