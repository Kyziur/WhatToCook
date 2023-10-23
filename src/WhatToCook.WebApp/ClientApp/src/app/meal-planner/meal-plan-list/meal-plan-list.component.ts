import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { shorten } from '../../common/shorten';
import { MealPlanningService } from '../meal-planning.service';
import { map } from 'rxjs';

export interface MealPlanItem {
  id: number;
  name: string;
  fromDate: Date;
  toDate: Date;
  numberOfRecipes: number;
}

@Component({
  selector: 'app-meal-plan-list',
  templateUrl: './meal-plan-list.component.html',
  styleUrls: ['./meal-plan-list.component.scss'],
})
export class MealPlanListComponent {
  constructor(
    private router: Router,
    private service: MealPlanningService
  ) {}

  getPlans$ = this.service.getMealPlans().pipe(
    map(plans =>
      plans.map(
        plan =>
          ({
            ...plan,
            numberOfRecipes: plan.plannedMealsForDay.length,
          }) as MealPlanItem
      )
    )
  );

  public shortenName(name: string) {
    return shorten(name, 30);
  }

  showShoppingList(mealPlan: MealPlanItem) {}

  navigateToMealPlan(mealPlan: MealPlanItem) {
    this.router.navigate([`/meal-plans/${mealPlan.id}`]);
  }
}
