// export function distinct<T>(array: Array<T>, key: keyof T): Array<T> {
//   return [...new Map(array.map((item) => [item[key], item])).values()];
// }

export function distinct(array: Array<string>): Array<string> {
  return [...new Set(array)];
}

export function containsIgnoreCase(text: string, includes: string) {
  return text.toLowerCase().includes(includes.toLowerCase());
}
