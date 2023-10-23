import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MealPlanningService } from '../meal-planning.service';
import {
  PlanOfMeals,
  UpdatePlanOfMeals,
} from '../meal-plan-creator/plan-of-meals';

const EMPTY_MEAL_PLAN: PlanOfMeals = {
  name: 'Zaznacz',
  id: -1,
  plannedMealsForDay: [],
  toDate: new Date(),
  fromDate: new Date(),
};

@Component({
  selector: 'app-plan-select',
  templateUrl: './plan-select.component.html',
  styleUrls: ['./plan-select.component.scss'],
})
export class PlanSelectComponent {
  mealPlans: PlanOfMeals[] = [];
  selectedMealPlan?: PlanOfMeals;

  @Output() onSelectionChange: EventEmitter<PlanOfMeals> =
    new EventEmitter<PlanOfMeals>();

  constructor(private mealPlanService: MealPlanningService) {}

  ngOnInit(): void {
    this.mealPlanService.getMealPlans().subscribe({
      next: mealPlans => {
        this.mealPlans = [EMPTY_MEAL_PLAN, ...mealPlans];
        this.selectedMealPlan = this.mealPlans[0];
      },
      error: err => console.error(err),
    });
  }

  onSubmit() {
    if (!this.selectedMealPlan) {
      return;
    }
    const mealPlan: UpdatePlanOfMeals = {
      id: this.selectedMealPlan.id,
      fromDate: this.selectedMealPlan.fromDate,
      toDate: this.selectedMealPlan.toDate,
      name: this.selectedMealPlan.name,
      recipes: [],
      //   this.mealPlanService.selectedRecipes.map(recipe => {
      //   return recipe.name;
      // }),
    };
    this.mealPlanService.update(mealPlan).subscribe();
  }
}
