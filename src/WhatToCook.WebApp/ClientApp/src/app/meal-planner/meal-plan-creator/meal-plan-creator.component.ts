import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Badge } from '../../shared/badge/badge.component';
import { MealPlanningService } from '../meal-planning.service';
import { of, Subject, switchMap, takeUntil } from 'rxjs';
import { MealPlanForDay } from './models/meal-plan-for.day';
import {
  createMalPlanForm,
  createMealPlanForDayForm,
  MealPlanForDayForm,
  MealPlanForm,
} from './models/meal-plan-form';
import {
  CreatePlanOfMealApi,
  PlanOfMealForDayApi,
  UpdatePlanOfMealApi,
} from '../api-models/plan-of-meal.model';
import { generateRangeOfDates } from '../../common/functions/date';
import { RecipeListService } from '../../recipes/recipe-list/recipe-list.service';

@Component({
  selector: 'app-meal-creator',
  templateUrl: './meal-plan-creator.component.html',
  styleUrls: ['./meal-plan-creator.component.scss'],
})
export class MealPlanCreatorComponent implements OnInit, OnDestroy {
  public selectedMealPlanForDay?: MealPlanForDay;
  public mealPlanForm: FormGroup<MealPlanForm> = createMalPlanForm(this.fb);
  private destroy$ = new Subject<boolean>();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private service: MealPlanningService,
    private route: ActivatedRoute,
    private recipeListService: RecipeListService
  ) {
    this.setUpOnDatesUpdateHandler();
    this.setUpOnSelectedRecipesHandler();
  }

  ngOnInit(): void {
    this.route.params
      .pipe(
        takeUntil(this.destroy$),
        switchMap(params => {
          const name = params['name'] as string;
          return name ? this.service.getByName(name) : of(undefined);
        })
      )
      .subscribe(mealPlan => {
        if (!mealPlan) {
          return;
        }

        this.mealPlanForm.patchValue({
          id: mealPlan.id,
          dates: {
            from: mealPlan.fromDate,
            to: mealPlan.toDate,
          },
          name: mealPlan.name,
          plannedMealsForDay: mealPlan.recipes,
        });
        this.updateDaysToPlanBasedOnSelectedDates();
        console.error('meal plan updated', this.mealPlanForm.value);
      });
  }
  ngOnDestroy(): void {
    this.destroy$.next(true);
    this.destroy$.unsubscribe();
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

  updateDaysToPlanBasedOnSelectedDates() {
    const plannedMeals = this.getDaysFromSelectedDates().map(day => {
      const previouslySelectedRecipesIds =
        this.mealPlanForm.controls.plannedMealsForDay.value.find(
          x => x.day?.getDate() === day.getDate()
        )?.recipesIds ?? [];

      return createMealPlanForDayForm(day, previouslySelectedRecipesIds);
    });

    this.mealPlanForm.controls.plannedMealsForDay =
      this.fb.nonNullable.array<FormGroup<MealPlanForDayForm>>(plannedMeals);
  }
  setUpOnDatesUpdateHandler() {
    this.mealPlanForm.controls.dates.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.selectedMealPlanForDay = undefined;
        this.updateDaysToPlanBasedOnSelectedDates();
      });
  }

  setUpOnSelectedRecipesHandler() {
    this.recipeListService
      .getSelected$()
      .pipe(takeUntil(this.destroy$))
      .subscribe(cards => {
        if (!this.selectedMealPlanForDay) {
          return;
        }
        this.selectedMealPlanForDay.recipesIds = cards.map(x => x.id);
      });
  }

  handleSuccessfulSave() {
    this.router.navigate(['recipes']);
  }

  submit() {
    const isFormInvalid = !this.mealPlanForm || this.mealPlanForm.invalid;

    if (isFormInvalid) {
      return;
    }

    if (!this.fromDate.value || !this.toDate.value) {
      return;
    }

    if (this.mealPlanForm.value.id) {
      const updateRequest: UpdatePlanOfMealApi = {
        id: this.mealPlanForm.value.id,
        name: this.name.value,
        fromDate: new Date(this.fromDate.value),
        toDate: new Date(this.toDate.value),
        recipes: this.mealPlanForm.controls.plannedMealsForDay.value.filter(
          x => x.day !== undefined
        ) as PlanOfMealForDayApi[],
      };

      this.service
        .update(updateRequest)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => this.handleSuccessfulSave());
      return;
    }
    const createRequest: CreatePlanOfMealApi = {
      name: this.name.value,
      fromDate: new Date(this.fromDate.value),
      toDate: new Date(this.toDate.value),
      recipes: this.mealPlanForm.controls.plannedMealsForDay.value.filter(
        x => x.day !== undefined
      ) as PlanOfMealForDayApi[],
    };

    this.service
      .create(createRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.handleSuccessfulSave());
  }

  getDaysFromSelectedDates() {
    const fromDate = this.fromDate.value;
    const toDate = this.toDate.value;
    const isValidAndHasValues =
      this.mealPlanForm.controls.dates.valid && fromDate && toDate;

    if (!isValidAndHasValues) {
      return [];
    }
    return generateRangeOfDates(new Date(fromDate), new Date(toDate));
  }

  selectedDayChangedHandler($event: MouseEvent, day: Date) {
    $event.preventDefault();

    this.selectedMealPlanForDay =
      this.mealPlanForm.controls.plannedMealsForDay.value.find(
        x => x.day === day
      ) as MealPlanForDay;

    this.recipeListService.select(this.selectedMealPlanForDay.recipesIds);
  }

  getBadge(numberOfRecipes: number | undefined): Badge {
    return {
      text: (numberOfRecipes ?? 0).toString(),
      level: 'info',
    };
  }
}
