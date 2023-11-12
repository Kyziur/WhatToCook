import { Component, OnInit } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { CreatePlanOfMeals, PlanOfMealForDay } from './plan-of-meals';
import { ActivatedRoute, Router } from '@angular/router';
import {
  dateRangeValidator,
  notPastDateValidator,
} from './date-validators.component';
import { notWhitespaceValidator } from 'src/app/not-white-space-validator.component';
import { Badge } from '../../shared/badge/badge.component';
import { MealPlanForDay } from './meal-plan-for.day';
import { MealPlanningService } from '../meal-planning.service';
import { of, switchMap } from 'rxjs';
import {
  createMalPlanForm,
  createMealPlanForDayForm,
  MealPlanForDayForm,
  MealPlanForm,
} from './meal-plan-form.types';
@Component({
  selector: 'app-meal-creator',
  templateUrl: './meal-plan-creator.component.html',
  styleUrls: ['./meal-plan-creator.component.scss'],
})
export class MealPlanCreatorComponent implements OnInit {
  // public mealPlans: MealPlanForDay[] = [];
  public selectedMealPlanForDay?: MealPlanForDay;
  public mealPlanForm: FormGroup<MealPlanForm> = createMalPlanForm(this.fb);

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private service: MealPlanningService,
    private route: ActivatedRoute
  ) {
    this.mealPlanForm.controls.dates.valueChanges.subscribe(_ => {
      this.changedDateRangeHandler();

      const plannedMeals = this.getDaysFromSelectedDates().map(day => {
        const previouslySelectedRecipesIds =
          this.mealPlanForm.controls.plannedMealsForDay.value.find(
            x => x.day?.getDate() === day.getDate()
          )?.recipesIds ?? [];

        return createMealPlanForDayForm(day, previouslySelectedRecipesIds);
      });

      this.mealPlanForm.controls.plannedMealsForDay = new FormArray<
        FormGroup<MealPlanForDayForm>
      >(plannedMeals);
      // plannedMeals.forEach(plannedMeal =>
      //   this.mealPlanForm.controls.plannedMealsForDay.push(plannedMeal)
      // );

      // this.mealPlans = this.getDaysFromSelectedDates().map(day => {
      //   const previouslySelectedRecipes = this.mealPlans.find(
      //     x => x.day.getDate() === day.getDate()
      //   )?.recipes;
      //
      //   return {
      //     day: day,
      //     recipes: previouslySelectedRecipes ?? [],
      //     show: false,
      //   };
      // });
    });
  }

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

  ngOnInit(): void {
    this.route.params
      .pipe(
        switchMap(params => {
          const id = params['id'];
          if (!id) {
            return of(undefined);
          }
          return this.service.getMealPlanById(id);
        })
      )
      .subscribe(mealPlan => {
        if (!mealPlan) {
          return;
        }

        this.mealPlanForm.patchValue({
          id: mealPlan.id,
          dates: {
            from: mealPlan.fromDate.toDateString(),
            to: mealPlan.toDate.toDateString(),
          },
          name: mealPlan.name,
          plannedMealsForDay: mealPlan.plannedMealsForDay,
        });
      });
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
      recipes: this.mealPlanForm.controls.plannedMealsForDay.value.filter(
        x => x.day !== undefined
      ) as PlanOfMealForDay[],
    };

    // this.mealPlanService.createMealPlan(request).subscribe(_ => this.handleSuccessfulSave());
  }

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

  selectDayClickHandler($event: MouseEvent, day: Date) {
    $event.preventDefault();
    this.selectedMealPlanForDay =
      this.mealPlanForm.controls.plannedMealsForDay.value.find(
        x => x.day === day
      ) as MealPlanForDay;
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

  getBadge(numberOfRecipes: number | undefined): Badge {
    return {
      text: (numberOfRecipes ?? 0).toString(),
      level: 'info',
    };
  }
}
