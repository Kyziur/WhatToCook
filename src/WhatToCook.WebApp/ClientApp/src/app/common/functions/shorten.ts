export function shorten(text: string, maxLength: number) {
  if (text.length < maxLength) {
    return text;
  }

  const nameParts = text.split(' ');
  let finalName = '';
  for (const namePart of nameParts) {
    if (finalName.length > maxLength) {
      return `${finalName}...`;
    }

    finalName = `${finalName} ${namePart}`;
  }

  return finalName;
}
