import { Component, OnInit } from '@angular/core';
import { MealPlanningService } from '../meal-planning.service';
import { shoppingListResponse } from './shopping-list-response.component';
import { GetPlanOfMealApi } from '../api-models/plan-of-meal.model';
@Component({
  selector: 'app-shopping-list',
  templateUrl: './shopping-list.component.html',
  styleUrls: ['./shopping-list.component.scss'],
})
export class ShoppingListComponent implements OnInit {
  viewEntireList = false;
  entireShoppingList: string[] = [];
  mealPlans: GetPlanOfMealApi[] = [];
  selectedMealPlanId: number | null = null;
  shoppingList: shoppingListResponse = {
    fromDate: new Date(),
    toDate: new Date(),
    ingredientsPerDay: [],
  };
  constructor(private mealPlanService: MealPlanningService) {}

  ngOnInit(): void {
    this.fetchMealPlans();
  }

  fetchMealPlans(): void {
    this.mealPlanService.getAll().subscribe(plans => {
      this.mealPlans = plans.mealPlans;
    });
  }

  onPlanSelected(): void {
    if (this.selectedMealPlanId !== null) {
      this.mealPlanService
        .getIngredientsForShoppingList(this.selectedMealPlanId)
        .subscribe(response => {
          this.shoppingList = response;

          if (this.viewEntireList) {
            this.entireShoppingList = [];
            for (const dayIngredients of response.ingredientsPerDay) {
              this.entireShoppingList.push(...dayIngredients.ingredients);
            }
          }
        });
    }
  }
}
