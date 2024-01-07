import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  CreatePlanOfMealApi,
  GetPlanOfMealApi,
  GetPlansOfMealsApi,
  UpdatePlanOfMealApi,
} from './api-models/plan-of-meal.model';
import { Observable } from 'rxjs';
import { ShoppingListResponse } from './shopping-list/shopping-list-response.component';

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

  create(planOfMeal: CreatePlanOfMealApi) {
    return this.httpClient.post<CreatePlanOfMealApi>(
      this.baseUrl + 'api/v1/MealPlanning',
      planOfMeal
    );
  }

  update(planOfMeal: UpdatePlanOfMealApi) {
    return this.httpClient.put<UpdatePlanOfMealApi>(
      this.baseUrl + 'api/v1/MealPlanning',
      planOfMeal
    );
  }

  getByName(name: string): Observable<GetPlanOfMealApi> {
    return this.httpClient.get<GetPlanOfMealApi>(
      `${this.baseUrl}api/v1/MealPlanning/${name}`
    );
  }

  getAll(): Observable<GetPlansOfMealsApi> {
    return this.httpClient.get<GetPlansOfMealsApi>(
      `${this.baseUrl}api/v1/MealPlanning`
    );
  }

  getIngredientsForShoppingList(id: number): Observable<ShoppingListResponse> {
    return this.httpClient.get<ShoppingListResponse>(
      `${this.baseUrl}api/v1/MealPlanning/GetShoppingList/${id}`
    );
  }
}
