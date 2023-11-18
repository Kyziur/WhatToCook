import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MealPlanningService } from '../meal-planning.service';
import { map } from 'rxjs';
import { shorten } from '../../common/functions/shorten';

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

  getPlans$ = this.service.getAll().pipe(
    map(response =>
      response.mealPlans.map(
        plan =>
          ({
            ...plan,
            numberOfRecipes: plan.recipes.length,
          }) as MealPlanItem
      )
    )
  );

  public shortenName(name: string) {
    return shorten(name, 30);
  }

  showShoppingList(mealPlan: MealPlanItem) {}

  navigateToMealPlan(mealPlan: MealPlanItem) {
    this.router.navigate([`/meal-plans/${mealPlan.name}`]);
  }
}
