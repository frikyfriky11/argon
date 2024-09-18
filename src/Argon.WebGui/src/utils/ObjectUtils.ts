export function resolveJsonPath(path: string, obj: unknown, separator = ".") {
  // input can be something like "myObj.prop1.nestedProp2", we need the properties separated
  const properties = path.split(separator);

  // start from the object
  let currentValue = obj;

  // iterate over each property in the path
  for (const property of properties) {
    // if the current object has the property
    // eslint-disable-next-line no-prototype-builtins
    if (currentValue?.hasOwnProperty(property)) {
      // start again but this time from the object accessed by the property in the path
      currentValue = (currentValue as Record<string, unknown>)[property];
    } else {
      // cannot find anything with the path provided, return null
      return null;
    }
  }

  // return the last accessed value with the path provided
  return currentValue;
}
