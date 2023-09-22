import { useSearchParams } from "react-router-dom";

type ParamType = string | number | boolean | (string | number)[];

/**
 * Sets the value of a search parameter in the URL and returns the current state of the search parameter.
 * @param {string} searchParamName - The name of the search parameter to set.
 * @param {T} defaultValue - The default value for the search parameter.
 * @return {readonly [T, (newState: T) => void]} - An array containing the current state of the search parameter and a function to set the new state.
 */
export default function useSearchParamsState<T extends ParamType>(
  searchParamName: string,
  defaultValue: T,
): readonly [
  searchParamsState: T,
  setSearchParamsState: (newState: T) => void,
] {
  const [searchParams, setSearchParams] = useSearchParams();

  // try to get the parameter from the URL
  const acquiredSearchParam = searchParams.get(searchParamName);

  let searchParamsState: T;

  // try to parse the parameter as an array, a number, a boolean or a string
  // if the parameter is not set or if it is impossible to parse, use the default value
  try {
    if (Array.isArray(defaultValue)) {
      searchParamsState = (
        acquiredSearchParam !== null
          ? JSON.parse(acquiredSearchParam)
          : defaultValue
      ) as T;
    } else if (typeof defaultValue === "number") {
      searchParamsState = (
        acquiredSearchParam !== null
          ? Number(acquiredSearchParam)
          : defaultValue
      ) as T;
    } else if (typeof defaultValue === "boolean") {
      // having ?param or ?param=true should both be considered as a true value
      searchParamsState = (
        acquiredSearchParam !== null
          ? acquiredSearchParam == "" || acquiredSearchParam === "true"
          : defaultValue
      ) as T;
    } else {
      searchParamsState = (acquiredSearchParam ?? defaultValue) as T;
    }
  } catch (error) {
    // if we can't parse the URL state, just return the default value
    console.error(
      `Error caught while trying to parse URL state with key ${searchParamName}:`,
      error,
    );

    searchParamsState = defaultValue;
  }

  /**
   * Updates the search parameters state with a new state value.
   *
   * @param {T} newState - The new state value.
   */
  const setSearchParamsState = (newState: T) => {
    let newSearchValue;

    // if it is an array, we need to stringify it before storing it as state
    // all the other types should have the toString method built-in so we can use that
    if (Array.isArray(newState)) {
      newSearchValue = JSON.stringify(newState);
    } else {
      newSearchValue = newState.toString();
    }

    // create a new object with the current state and add the new value to it
    const next = Object.assign(
      {},
      [...searchParams.entries()].reduce(
        (o, [key, value]) => ({ ...o, [key]: value }),
        {},
      ),
      { [searchParamName]: newSearchValue },
    );

    setSearchParams(next);
  };

  // return the hook like useState does
  return [searchParamsState, setSearchParamsState] as const;
}
