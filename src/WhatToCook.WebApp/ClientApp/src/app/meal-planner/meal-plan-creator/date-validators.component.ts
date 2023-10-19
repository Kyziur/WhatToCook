import { AbstractControl, FormGroup, ValidationErrors } from '@angular/forms';
import { MealPlanFormDates } from './meal-plan-creator.component';

export function dateRangeValidator(
  group: AbstractControl
): ValidationErrors | null {
  const datesForm = group as unknown as FormGroup<MealPlanFormDates>;
  const error = { dateMismatch: true };
  if (!datesForm.controls.from.value || !datesForm.controls.to.value) {
    return error;
  }

  const fromDate = new Date(datesForm.controls.from.value);
  const toDate = new Date(datesForm.controls.to.value);
  return toDate >= fromDate ? null : error;
}

export function notPastDateValidator(
  control: AbstractControl
): ValidationErrors | null {
  const selectedDate = new Date(control.value);
  const currentDate = new Date();
  currentDate.setHours(0, 0, 0, 0); // Reset time to start of day
  return selectedDate >= currentDate ? null : { pastDate: true };
}
