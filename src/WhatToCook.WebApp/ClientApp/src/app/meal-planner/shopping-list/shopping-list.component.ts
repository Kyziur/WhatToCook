import { Component, OnInit } from '@angular/core';
import { MealPlanningService } from '../meal-planning.service';
import { shoppingListResponse } from './shopping-list-response.component';
import { PlanOfMeals } from '../meal-plan-creator/plan-of-meals';
@Component({
  selector: 'app-shopping-list',
  templateUrl: './shopping-list.component.html',
  styleUrls: ['./shopping-list.component.scss'],
})
export class ShoppingListComponent implements OnInit {
  mealPlans: PlanOfMeals[] = [];
  selectedMealPlanId: number | null = null;
  shoppingList: shoppingListResponse = {
    ingredients: [],
    fromDate: new Date(),
    toDate: new Date(),
    dayWiseIngredientsList: [],
  };
  constructor(private mealPlanService: MealPlanningService) {}

  ngOnInit(): void {
    this.fetchMealPlans();
  }

  fetchMealPlans(): void {
    this.mealPlanService.getMealPlans().subscribe(plans => {
      this.mealPlans = plans;
    });
  }

  onPlanSelected(): void {
    if (this.selectedMealPlanId) {
      this.mealPlanService
        .getIngredientsForShoppingList(this.selectedMealPlanId)
        .subscribe(response => {
          this.shoppingList = response;
        });
    }
  }
}
