import { Component } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { CreatePlanOfMeals } from './plan-of-meals';
import { Router } from '@angular/router';
import {
  dateRangeValidator,
  notPastDateValidator,
} from './date-validators.component';
import { notWhitespaceValidator } from 'src/app/not-white-space-validator.component';
import { RecipeListService } from '../../recipes/recipe-list/recipe-list.service';
import { RecipeCard } from '../../recipes/recipe-card/recipe-card.component';
import { Badge } from '../../shared/badge/badge.component';

export interface MealPlanFormDates {
  from: FormControl<string | null>;
  to: FormControl<string | null>;
}

export interface MealPlanForm {
  name: FormControl<string>;
  dates: FormGroup<MealPlanFormDates>;
  recipes: FormArray<FormControl<string>>;
}

@Component({
  selector: 'app-meal-creator',
  templateUrl: './meal-plan-creator.component.html',
  styleUrls: ['./meal-plan-creator.component.scss'],
})
export class MealPlanCreatorComponent {
  public mealPlans: MealPlanForDay[] = [];
  public selectedMealPlanForDay?: MealPlanForDay;

  mealPlanForm: FormGroup<MealPlanForm> = new FormGroup({
    name: this.fb.nonNullable.control('', [
      Validators.required,
      notWhitespaceValidator,
    ]),
    dates: this.fb.nonNullable.group(
      {
        from: this.fb.control<string | null>(null, [
          Validators.required,
          notPastDateValidator,
        ]),
        to: this.fb.control<string | null>(null, [
          Validators.required,
          notPastDateValidator,
        ]),
      },
      { validators: dateRangeValidator }
    ),
    recipes: this.fb.nonNullable.array([] as FormControl<string>[]),
  });

  get fromDate() {
    return this.mealPlanForm.controls.dates.controls.from;
  }

  get toDate() {
    return this.mealPlanForm.controls.dates.controls.to;
  }

  get name() {
    return this.mealPlanForm.controls.name;
  }

  handleSuccessfulSave() {
    this.router.navigate(['recipes']);
  }

  constructor(
    private fb: FormBuilder,
    private router: Router,
    public recipeListService: RecipeListService
  ) {
    this.mealPlanForm.controls.dates.valueChanges.subscribe(_ => {
      this.changedDateRangeHandler();
      this.mealPlans = this.getDaysFromSelectedDates().map(day => ({
        day,
        recipes: [],
        show: false,
      }));
    });

    this.mealPlanForm.controls.dates.patchValue({
      from: '2023-10-16',
      to: '2023-10-18',
    });

    this.recipeListService.showSelectButton = true;
  }

  submit() {
    if (!this.mealPlanForm || this.mealPlanForm.invalid) {
      return;
    }

    if (!this.fromDate.value || !this.toDate.value) {
      return;
    }

    const request: CreatePlanOfMeals = {
      name: this.name.value,
      fromDate: new Date(this.fromDate.value),
      toDate: new Date(this.toDate.value),
      recipes: this.mealPlans.map(m => {
        return {
          day: m.day,
          recipeIds: m.recipes.map(r => r.id),
        };
      }),
    };

    console.error('request', request);

    // this.mealPlanService.createMealPlan(request).subscribe(_ => this.handleSuccessfulSave());
  }

  //TODO: CHANGE CONCEPT - GENERATE RIGHT AWAY THE mealPlanForDay[] based on the days that were selected
  getDaysFromSelectedDates() {
    const fromDate = this.fromDate.value;
    const toDate = this.toDate.value;

    const isValidAndHasValues =
      this.mealPlanForm.controls.dates.valid &&
      fromDate !== null &&
      toDate !== null;

    if (!isValidAndHasValues) {
      return [];
    }

    return this.generateRangeOfDates(new Date(fromDate), new Date(toDate));
  }

  selectDayClickHandler($event: MouseEvent, mealPlanForDay: MealPlanForDay) {
    $event.preventDefault();

    if (!this.selectedMealPlanForDay) {
      this.saveCurrentlySelectedRecipes(mealPlanForDay);
    } else {
      this.saveCurrentlySelectedRecipes(this.selectedMealPlanForDay);
    }

    this.selectedMealPlanForDay = mealPlanForDay;
    this.selectRecipesForSelectedDay(this.selectedMealPlanForDay);
  }

  selectRecipesForSelectedDay(mealPlanForDay: MealPlanForDay) {
    this.recipeListService.recipeCards = this.recipeListService.recipeCards.map(
      recipeCard => {
        recipeCard.isSelected =
          mealPlanForDay.recipes.includes(recipeCard) ?? false;
        return recipeCard;
      }
    );
  }

  saveCurrentlySelectedRecipes(mealPlanForDay: MealPlanForDay) {
    mealPlanForDay.recipes = this.recipeListService.getSelectedRecipes();
  }

  changedDateRangeHandler() {
    this.selectedMealPlanForDay = undefined;
  }

  generateRangeOfDates(from: Date, to: Date) {
    const currentDate = from;
    const rangeDates: Date[] = [];
    while (currentDate <= to) {
      rangeDates.push(new Date(currentDate));
      currentDate.setDate(currentDate.getDate() + 1);
    }
    return rangeDates;
  }

  getBadge(mealPlan: MealPlanForDay): Badge {
    return {
      text: mealPlan.recipes.length.toString(),
      level: 'info',
    };
  }
}

export interface MealPlanForDay {
  day: Date;
  //TODO: CHANGE LATER ON TO RECIPE OR SOMETHING ELSE
  recipes: RecipeCard[];
  show: boolean;
}
