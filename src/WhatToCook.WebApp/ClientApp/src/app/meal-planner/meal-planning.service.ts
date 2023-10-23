import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreatePlanOfMeals,
  PlanOfMeals,
  UpdatePlanOfMeals,
} from './meal-plan-creator/plan-of-meals';
import { shoppingListResponse } from './shopping-list/shopping-list-response.component';

@Injectable({
  providedIn: 'root',
})
export class MealPlanningService {
  mealPlanUrl = '';

  constructor(
    private httpClient: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {
    this.mealPlanUrl = this.baseUrl + 'api/v1/MealPlanning';
  }

  createMealPlan(planOfMeals: CreatePlanOfMeals) {
    return this.httpClient.post<PlanOfMeals>(
      this.baseUrl + 'api/v1/MealPlanning',
      planOfMeals
    );
  }

  getMealPlans(): Observable<PlanOfMeals[]> {
    return this.httpClient.get<PlanOfMeals[]>(
      `${this.baseUrl}api/v1/MealPlanning`
    );
  }
  update(planOfMeals: UpdatePlanOfMeals) {
    return this.httpClient.put<UpdatePlanOfMeals>(
      this.baseUrl + 'api/v1/MealPlanning',
      planOfMeals
    );
  }

  getMealPlanById(id: number): Observable<PlanOfMeals> {
    return this.httpClient.get<PlanOfMeals>(
      `${this.baseUrl}api/v1/MealPlanning/${id}`
    );
  }
  getIngredientsForShoppingList(id: number): Observable<shoppingListResponse> {
    return this.httpClient.get<shoppingListResponse>(
      `${this.baseUrl}api/v1/MealPlanning/GetShoppingList/${id}`
    );
  }
}
