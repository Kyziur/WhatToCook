import { AbstractControl, ValidationErrors } from '@angular/forms';

export function dateRangeValidator(
  group: AbstractControl,
): ValidationErrors | null {
  const fromDate = new Date(group.get('fromDate')?.value);
  const toDate = new Date(group.get('toDate')?.value);
  return toDate >= fromDate ? null : { dateMismatch: true };
}

export function notPastDateValidator(
  control: AbstractControl,
): ValidationErrors | null {
  const selectedDate = new Date(control.value);
  const currentDate = new Date();
  currentDate.setHours(0, 0, 0, 0); // Reset time to start of day
  return selectedDate >= currentDate ? null : { pastDate: true };
}
