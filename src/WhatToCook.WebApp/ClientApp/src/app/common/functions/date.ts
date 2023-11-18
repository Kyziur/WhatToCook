export function generateRangeOfDates(from: Date, to: Date) {
  const currentDate = from;
  const rangeDates: Date[] = [];
  while (currentDate <= to) {
    rangeDates.push(new Date(currentDate));
    currentDate.setDate(currentDate.getDate() + 1);
  }
  return rangeDates;
}

/**
 * Formats date to YYYY-MM-DD format
 * @param date
 */
export function formatDate(date: Date) {
  const d = new Date(date);
  let month = '' + (d.getMonth() + 1);
  let day = '' + d.getDate();
  const year = d.getFullYear();
  if (month.length < 2) month = '0' + month;
  if (day.length < 2) day = '0' + day;
  return [year, month, day].join('-');
}
