import { DateTime } from "luxon";
import { useSearchParams } from "react-router-dom";

/**
 * Hook for managing multiple search parameters as a single state object.
 */
export default function useSearchParamsState<T extends Record<string, unknown>>(
  defaultParams: T,
): readonly [T, (newParams: (prevValue: T) => T) => void] {
  const [searchParams, setSearchParams] = useSearchParams();

  const currentParams: object = {};

  for (const key in defaultParams) {
    const acquiredSearchParam = searchParams.get(key);

    if (acquiredSearchParam !== null) {
      try {
        if (Array.isArray(defaultParams[key])) {
          Object.assign(currentParams, {
            [key]: JSON.parse(acquiredSearchParam) as never[],
          });
        } else if (typeof defaultParams[key] === "number") {
          Object.assign(currentParams, { [key]: Number(acquiredSearchParam) });
        } else if (typeof defaultParams[key] === "boolean") {
          Object.assign(currentParams, {
            [key]: acquiredSearchParam === "" || acquiredSearchParam === "true",
          });
        } else if (DateTime.fromISO(acquiredSearchParam).isValid) {
          Object.assign(currentParams, {
            [key]: DateTime.fromISO(acquiredSearchParam),
          });
        } else {
          Object.assign(currentParams, { [key]: acquiredSearchParam });
        }
      } catch (error) {
        console.error(
          `Error caught while parsing URL state for key ${key}:`,
          error,
        );
      }
    } else {
      Object.assign(currentParams, { [key]: defaultParams[key] });
    }
  }

  // Batching updates for multiple search params
  const setSearchParamsState = (newParams: (prevValue: T) => T) => {
    const nextSearchParams = new URLSearchParams(searchParams);

    // Iterate over the newParams and update the URLSearchParams
    Object.keys(currentParams).forEach((key) => {
      const value = newParams(currentParams as T)[key];

      if (Array.isArray(value)) {
        if (value.length > 0) {
          nextSearchParams.set(key, JSON.stringify(value));
        } else {
          nextSearchParams.delete(key);
        }
      } else if (
        typeof value === "number" ||
        typeof value === "boolean" ||
        typeof value === "string"
      ) {
        if (value.toString() !== "") {
          nextSearchParams.set(key, value.toString());
        } else {
          nextSearchParams.delete(key);
        }
      } else if (DateTime.isDateTime(value)) {
        nextSearchParams.set(key, (value as DateTime).toISODate()!);
      } else {
        nextSearchParams.delete(key);
      }
    });

    setSearchParams(nextSearchParams);
  };

  return [currentParams as T, setSearchParamsState] as const;
}
