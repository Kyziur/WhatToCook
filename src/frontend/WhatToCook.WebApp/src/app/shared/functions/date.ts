export function generateRangeOfDates(from: Date, to: Date) {
  const currentDate = from;
  const rangeDates: Date[] = [];
  while (currentDate <= to) {
    rangeDates.push(new Date(currentDate));
    currentDate.setDate(currentDate.getDate() + 1);
  }
  return rangeDates;
}
