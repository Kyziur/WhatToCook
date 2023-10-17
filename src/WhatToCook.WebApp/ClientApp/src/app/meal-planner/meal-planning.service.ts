import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Recipe } from 'src/app/recipes/Recipe';
import { Observable } from 'rxjs';
import {
  PlanOfMeals,
  UpdatePlanOfMeals,
} from './meal-plan-creator/plan-of-meals';
import { ShoppingListResponse } from './shopping-list/shopping-list-response.component';

@Injectable({
  providedIn: 'root',
})
export class MealPlanningService {
  mealPlanUrl = '';
  selectedRecipes: Recipe[] = [];

  constructor(
    private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
  ) {
    this.mealPlanUrl = this.baseUrl + 'api/v1/MealPlanning';
  }

  createMealPlan(planOfMeals: PlanOfMeals) {
    return this.httpClient.post<PlanOfMeals>(
      this.baseUrl + 'api/v1/MealPlanning',
      planOfMeals,
    );
  }

  getMealPlans(): Observable<PlanOfMeals[]> {
    return this.httpClient.get<PlanOfMeals[]>(
      `${this.baseUrl}api/v1/MealPlanning`,
    );
  }
  update(planOfMeals: UpdatePlanOfMeals) {
    return this.httpClient.put<UpdatePlanOfMeals>(
      this.baseUrl + 'api/v1/MealPlanning',
      planOfMeals,
    );
  }
  selectRecipe(recipe: Recipe) {
    this.selectedRecipes.push(recipe);
  }
  getMealPlanById(id: number): Observable<PlanOfMeals> {
    return this.httpClient.get<PlanOfMeals>(
      `${this.baseUrl}api/v1/MealPlanning/${id}`,
    );
  }
  getIngredientsForShoppingList(id: number): Observable<ShoppingListResponse>{
    return this.httpClient.get<ShoppingListResponse>
    (`${this.baseUrl}api/v1/MealPlanning/GetIngredients/${id}`)
  }
}
