export function getEnumAsArray<T extends string, TEnumValue extends number>(
  enumVariable: { [key in T]: TEnumValue },
  converter: (value: number) => string,
): { text: string; value: number }[] {
  const values = Object.values(enumVariable);

  const numberValues = values.slice(
    values.length / 2,
    values.length,
  ) as number[];

  return numberValues.map((value) => ({
    text: converter(value),
    value,
  }));
}
