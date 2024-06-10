import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import {
  dateRangeValidator,
  notPastDateValidator,
} from './meal-plan-dates.validators';
import { notWhitespaceValidator } from '../../../common/validators/not-white-space-validator.component';

export interface MealPlanForm {
  id: FormControl<number | null>;
  name: FormControl<string>;
  dates: FormGroup<MealPlanDatesForm>;
  plannedMealsForDay: FormArray<FormGroup<MealPlanForDayForm>>;
}

export interface MealPlanDatesForm {
  from: FormControl<Date | null>;
  to: FormControl<Date | null>;
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

export function createMalPlanForm(fb: FormBuilder): FormGroup<MealPlanForm> {
  return new FormGroup<MealPlanForm>({
    id: fb.control<number | null>(null),
    name: fb.nonNullable.control('', {
      validators: [Validators.required, notWhitespaceValidator],
      updateOn: 'blur',
    }),
    dates: fb.group(
      {
        from: fb.nonNullable.control<Date | null>(null, {
          validators: [Validators.required],
        }),
        to: fb.nonNullable.control<Date | null>(null, [Validators.required]),
      },
      { validators: dateRangeValidator }
    ),
    plannedMealsForDay: fb.nonNullable.array(
      [] as FormGroup<MealPlanForDayForm>[]
    ),
  });
}
