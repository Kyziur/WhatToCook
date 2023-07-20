import { Inject, Injectable } from '@angular/core';
import { PlanOfMeals } from './plan-of-meals';
import { HttpClient } from '@angular/common/http';
import { Recipe } from 'src/app/recipes/Recipe';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MealPlanningService {

  mealPlanUrl = "";

  selectedRecipes: Recipe[] = [];
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
    this.mealPlanUrl = this.baseUrl + 'api/v1/MealPlanning';
  }
  
  createMealPlan(planOfMeals : PlanOfMeals){
    return this.httpClient.post<PlanOfMeals>(this.baseUrl + 'api/v1/MealPlanning', planOfMeals )
  }

  getMealPlan(): Observable<PlanOfMeals[]> {
    return this.httpClient.get<PlanOfMeals[]>(`${this.baseUrl}api/v1/MealPlanning`);
  }

  selectRecipe(recipe: Recipe){
    this.selectedRecipes.push(recipe);

  }
}
