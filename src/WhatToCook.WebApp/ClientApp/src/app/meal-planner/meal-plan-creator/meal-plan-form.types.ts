import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { notWhitespaceValidator } from '../../not-white-space-validator.component';
import {
  dateRangeValidator,
  notPastDateValidator,
} from './date-validators.component';

export interface MealPlanForm {
  id: FormControl<number | null>;
  name: FormControl<string>;
  dates: FormGroup<MealPlanDatesForm>;
  plannedMealsForDay: FormArray<FormGroup<MealPlanForDayForm>>;
}

export interface MealPlanDatesForm {
  from: FormControl<string | null>;
  to: FormControl<string | null>;
}

export interface MealPlanForDayForm {
  day: FormControl<Date>;
  recipesIds: FormControl<number[]>;
}

export function createMealPlanForDayForm(day: Date, recipesIds: number[]) {
  return new FormGroup<MealPlanForDayForm>({
    day: new FormControl<Date>(day, { nonNullable: true }),
    recipesIds: new FormControl(recipesIds, { nonNullable: true }),
  });
}

export function createMalPlanForm(fb: FormBuilder) {
  return new FormGroup({
    id: fb.control<number | null>(null),
    name: fb.nonNullable.control('', {
      validators: [Validators.required, notWhitespaceValidator],
      updateOn: 'blur',
    }),
    dates: fb.nonNullable.group(
      {
        from: fb.control<string | null>(null, {
          validators: [Validators.required, notPastDateValidator],
          updateOn: 'blur',
        }),
        to: fb.control<string | null>(null, [
          Validators.required,
          notPastDateValidator,
        ]),
      },
      { validators: dateRangeValidator, updateOn: 'blur' }
    ),
    plannedMealsForDay: fb.nonNullable.array(
      [] as FormGroup<MealPlanForDayForm>[]
    ),
  });
}
